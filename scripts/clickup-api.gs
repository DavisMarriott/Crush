// ============================================
// DAVIS'S CLICKUP API v3 - Google Apps Script
// Uses GET with query params (POST doesn't survive Google's redirects)
// Pattern: same as calendar-access.gs
// v3: adds full checklist CRUD (get_task, update/delete items, edit/delete checklist)
//     + Crush space + crush-* list aliases
// ============================================

const API_KEY = "claude062312atwater";
const CLICKUP_BASE = "https://api.clickup.com/api/v2";

// --- Lookup Tables ---

const LISTS = {
  "backlog":        "901001573354",
  "recurring":      "900901642573",

  // Crush space (active — source of truth for Crush work)
  "crush-cards":            "901415489120",
  "crush-game-feel":        "901415491850",
  "crush-self-talk":        "901415489122",
  "crush-dialogue":         "901415489127",
  "crush-drafting":         "901415489257",
  "crush-base-loop":        "901415489258",
  "crush-early-loops":      "901415489260",
  "crush-special-loops":    "901415489261",
  "crush-final-loop":       "901415489262",
  "crush-death":            "901415492949",
  "crush-luke":             "901415489303",
  "crush-daisy":            "901415489304",
  "crush-environment":      "901415489305",
  "crush-cinematics":       "901415489307",
  "crush-additional-chars": "901415489310",
  "crush-game-shell":       "901415489423",
  "crush-tools":            "901415489428",
  "crush-debug":            "901415489429",
  "crush-licensing":        "901415490231",
  "crush-inbox":            "901415489529",

  // Legacy — do NOT use for new Crush work (kept for compatibility)
  "work-crush":     "901414101244",
  "work-backlog":   "901413237910",
  "work-learning":  "901407588561",
  "work-recurring": "901407766699"
};

const SPACES = {
  "personal": "90100265299",
  "crush":    "90145107347",
  "work":     "90142510405",
  "seeds":    "90142682962"
};

const FIELDS = {
  "this_week": "570d5618-5117-4909-93ec-8e96e05a70c9",
  "today":     "73f8f493-aea3-4637-b684-af6a6630cd07",
  "method":    "f025abe9-b8d0-4b77-a7d4-28632f794b5b"
};

const METHOD_VALUES = {
  "desk":  "72be987d-9130-4085-846c-605009cfefe8",
  "house": "775becac-9dc1-43e9-baee-4301801b93e2",
  "out":   "04ee4922-fed6-47ce-bc97-7b6fca295374"
};

const PRIORITIES = {
  "urgent": 1,
  "high":   2,
  "normal": 3,
  "low":    4
};

// --- Entry Point ---

function doGet(e) {
  try {
    var params = e.parameter;

    if (params.key !== API_KEY) {
      return jsonResponse({ error: "Unauthorized" });
    }

    // Support JSON payload via "data" param (for complex requests)
    var data = params;
    if (params.data) {
      data = JSON.parse(decodeURIComponent(params.data));
      data.key = params.key;
    }

    var action = data.action || params.action;

    switch (action) {
      case "filtered_tasks":
        return filteredTasks(data);
      case "update_field":
        return updateField(data);
      case "batch_update_field":
        return batchUpdateField(data);
      case "update_task":
        return updateTask(data);
      case "create_task":
        return createTask(data);
      case "get_task":
        return getTask(data);
      case "create_checklist":
        return createChecklist(data);
      case "add_checklist_items":
        return addChecklistItems(data);
      case "create_checklist_with_items":
        return createChecklistWithItems(data);
      case "update_checklist_item":
        return updateChecklistItem(data);
      case "delete_checklist_item":
        return deleteChecklistItem(data);
      case "edit_checklist":
        return editChecklist(data);
      case "delete_checklist":
        return deleteChecklist(data);
      case "ping":
        return jsonResponse({ status: "ok", message: "ClickUp API v3 is working!" });
      default:
        return jsonResponse({ error: "Unknown action: " + action });
    }
  } catch (err) {
    return jsonResponse({ error: err.message, stack: err.stack });
  }
}

// --- Actions ---

/**
 * filtered_tasks: Query tasks from a list with custom field filters.
 *
 * Params:
 *   list (required) - alias or raw list ID
 *   statuses - comma-separated status names (e.g. "open,in progress")
 *   this_week - "true" or "false" to filter by This Week checkbox
 *   today - "true" or "false" to filter by Today checkbox
 *   method - "desk", "house", or "out"
 *   due_before - ISO date string (tasks due before this date)
 *   due_after - ISO date string (tasks due after this date)
 *   include_closed - "true" to include closed tasks
 *   subtasks - "true" to include subtasks
 *   page - page number (0-indexed, 100 tasks per page)
 */
function filteredTasks(data) {
  var listId = resolveList(data.list);
  if (!listId) {
    return jsonResponse({ error: "Unknown list: " + data.list });
  }

  // Build query params
  var qp = [];

  // Statuses
  if (data.statuses) {
    var statuses = data.statuses.split(",");
    statuses.forEach(function(s) {
      qp.push("statuses[]=" + encodeURIComponent(s.trim()));
    });
  }

  // Include closed
  if (data.include_closed === "true" || data.include_closed === true) {
    qp.push("include_closed=true");
  }

  // Subtasks
  if (data.subtasks === "true" || data.subtasks === true) {
    qp.push("subtasks=true");
  }

  // Due date filters (ClickUp uses Unix ms timestamps)
  if (data.due_before) {
    qp.push("due_date_lt=" + new Date(data.due_before).getTime());
  }
  if (data.due_after) {
    qp.push("due_date_gt=" + new Date(data.due_after).getTime());
  }

  // Page
  if (data.page) {
    qp.push("page=" + data.page);
  }

  // Custom field filters
  var cfFilters = [];

  if (data.this_week !== undefined) {
    var twVal = (data.this_week === "true" || data.this_week === true);
    cfFilters.push({
      field_id: FIELDS["this_week"],
      operator: "=",
      value: twVal
    });
  }

  if (data.today !== undefined) {
    var todayVal = (data.today === "true" || data.today === true);
    cfFilters.push({
      field_id: FIELDS["today"],
      operator: "=",
      value: todayVal
    });
  }

  if (data.method) {
    var methodUuid = METHOD_VALUES[data.method];
    if (!methodUuid) {
      return jsonResponse({ error: "Unknown method: " + data.method + ". Use: desk, house, out" });
    }
    cfFilters.push({
      field_id: FIELDS["method"],
      operator: "ANY",
      value: [methodUuid]
    });
  }

  if (cfFilters.length > 0) {
    qp.push("custom_fields=" + encodeURIComponent(JSON.stringify(cfFilters)));
  }

  var endpoint = "/list/" + listId + "/task" + (qp.length ? "?" + qp.join("&") : "");
  var result = clickupRequest("GET", endpoint);

  // Slim down the response to essential fields
  var tasks = (result.tasks || []).map(function(t) {
    return {
      id: t.id,
      custom_id: t.custom_id || null,
      name: t.name,
      status: t.status ? t.status.status : null,
      priority: t.priority ? t.priority.priority : null,
      due_date: t.due_date ? new Date(parseInt(t.due_date)).toISOString() : null,
      start_date: t.start_date ? new Date(parseInt(t.start_date)).toISOString() : null,
      time_estimate: t.time_estimate || null,
      time_spent: t.time_spent ? parseInt(t.time_spent) : null,
      assignees: (t.assignees || []).map(function(a) { return a.username; }),
      tags: (t.tags || []).map(function(tag) { return tag.name; }),
      custom_fields: extractCustomFields(t.custom_fields || [])
    };
  });

  return jsonResponse({
    success: true,
    list: data.list,
    count: tasks.length,
    tasks: tasks
  });
}

/**
 * update_field: Set a single custom field on a task.
 *
 * Params:
 *   task_id (required) - ClickUp task ID
 *   field (required) - alias ("this_week", "today", "method") or raw field ID
 *   value (required) - the value to set. For checkboxes: true/false. For method: alias or UUID.
 */
function updateField(data) {
  if (!data.task_id) return jsonResponse({ error: "task_id required" });
  if (!data.field) return jsonResponse({ error: "field required" });

  var fieldId = FIELDS[data.field] || data.field;
  var value = resolveFieldValue(data.field, data.value);

  var result = clickupRequest("POST", "/task/" + data.task_id + "/field/" + fieldId, {
    value: value
  });

  return jsonResponse({
    success: true,
    task_id: data.task_id,
    field: data.field,
    value: value
  });
}

/**
 * batch_update_field: Set a field on multiple tasks at once.
 *
 * Params:
 *   task_ids (required) - comma-separated task IDs
 *   field (required) - alias or raw field ID
 *   value (required) - value to set
 */
function batchUpdateField(data) {
  if (!data.task_ids) return jsonResponse({ error: "task_ids required" });
  if (!data.field) return jsonResponse({ error: "field required" });

  var ids = data.task_ids.split(",").map(function(id) { return id.trim(); });
  var fieldId = FIELDS[data.field] || data.field;
  var value = resolveFieldValue(data.field, data.value);

  var results = [];
  ids.forEach(function(taskId) {
    try {
      clickupRequest("POST", "/task/" + taskId + "/field/" + fieldId, {
        value: value
      });
      results.push({ task_id: taskId, success: true });
    } catch (err) {
      results.push({ task_id: taskId, success: false, error: err.message });
    }
  });

  var successCount = results.filter(function(r) { return r.success; }).length;

  return jsonResponse({
    success: true,
    field: data.field,
    value: value,
    total: ids.length,
    updated: successCount,
    failed: ids.length - successCount,
    results: results
  });
}

/**
 * update_task: Update core task properties (name, status, priority, due_date).
 *
 * Params:
 *   task_id (required) - ClickUp task ID
 *   name - new task name
 *   status - new status string
 *   priority - alias ("urgent","high","normal","low") or number (1-4), or null to clear
 *   due_date - ISO date string, or null to clear
 *   start_date - ISO date string, or null to clear
 *   description - new description text
 */
function updateTask(data) {
  if (!data.task_id) return jsonResponse({ error: "task_id required" });

  var payload = {};
  if (data.name !== undefined) payload.name = data.name;
  if (data.status !== undefined) payload.status = data.status;
  if (data.description !== undefined) payload.description = data.description;

  if (data.priority !== undefined) {
    if (data.priority === null || data.priority === "null") {
      payload.priority = null;
    } else {
      payload.priority = PRIORITIES[data.priority] || parseInt(data.priority) || null;
    }
  }

  if (data.due_date !== undefined) {
    if (data.due_date === null || data.due_date === "null") {
      payload.due_date = null;
    } else {
      payload.due_date = parseDateToNoon(data.due_date);
      payload.due_date_time = false;
    }
  }

  if (data.start_date !== undefined) {
    if (data.start_date === null || data.start_date === "null") {
      payload.start_date = null;
    } else {
      payload.start_date = parseDateToNoon(data.start_date);
      payload.start_date_time = false;
    }
  }

  if (Object.keys(payload).length === 0) {
    return jsonResponse({ error: "No fields to update. Provide name, status, priority, due_date, start_date, or description." });
  }

  var result = clickupRequest("PUT", "/task/" + data.task_id, payload);

  return jsonResponse({
    success: true,
    task_id: data.task_id,
    updated: Object.keys(payload)
  });
}

/**
 * create_task: Create a new task in a list.
 *
 * Params:
 *   list (required) - alias or raw list ID
 *   name (required) - task name
 *   description - plain text description
 *   priority - alias or number
 *   due_date - ISO date string
 *   start_date - ISO date string
 *   status - status name
 *   assignee - user ID to assign (omit to leave unassigned)
 */
function createTask(data) {
  var listId = resolveList(data.list);
  if (!listId) return jsonResponse({ error: "Unknown list: " + data.list });
  if (!data.name) return jsonResponse({ error: "name required" });

  var payload = {
    name: data.name
  };

  if (data.description) payload.description = data.description;
  if (data.status) payload.status = data.status;

  if (data.priority) {
    payload.priority = PRIORITIES[data.priority] || parseInt(data.priority) || null;
  }

  if (data.due_date) {
    payload.due_date = parseDateToNoon(data.due_date);
    payload.due_date_time = false;
  }

  if (data.start_date) {
    payload.start_date = parseDateToNoon(data.start_date);
    payload.start_date_time = false;
  }

  if (data.assignee) {
    payload.assignees = [parseInt(data.assignee)];
  }

  var result = clickupRequest("POST", "/list/" + listId + "/task", payload);

  return jsonResponse({
    success: true,
    task_id: result.id,
    name: result.name,
    status: result.status ? result.status.status : null,
    list: data.list,
    url: result.url || null
  });
}

/**
 * get_task: Fetch a task with its checklists and items.
 *
 * Params:
 *   task_id (required) - ClickUp task ID
 *
 * Returns the task plus a slimmed checklist array. Each checklist includes
 * { id, name, orderindex, resolved, unresolved, items: [{ id, name, resolved, orderindex, parent }] }
 * so callers can discover IDs for update/delete operations.
 */
function getTask(data) {
  if (!data.task_id) return jsonResponse({ error: "task_id required" });

  var t = clickupRequest("GET", "/task/" + data.task_id);

  var checklists = (t.checklists || []).map(function(cl) {
    return {
      id: cl.id,
      name: cl.name,
      orderindex: cl.orderindex,
      resolved: cl.resolved,
      unresolved: cl.unresolved,
      items: (cl.items || []).map(function(it) {
        return {
          id: it.id,
          name: it.name,
          resolved: it.resolved,
          orderindex: it.orderindex,
          parent: it.parent || null
        };
      })
    };
  });

  return jsonResponse({
    success: true,
    task: {
      id: t.id,
      custom_id: t.custom_id || null,
      name: t.name,
      description: t.description || null,
      status: t.status ? t.status.status : null,
      priority: t.priority ? t.priority.priority : null,
      due_date: t.due_date ? new Date(parseInt(t.due_date)).toISOString() : null,
      start_date: t.start_date ? new Date(parseInt(t.start_date)).toISOString() : null,
      assignees: (t.assignees || []).map(function(a) { return { id: a.id, username: a.username }; }),
      tags: (t.tags || []).map(function(tag) { return tag.name; }),
      list: t.list ? { id: t.list.id, name: t.list.name } : null,
      folder: t.folder ? { id: t.folder.id, name: t.folder.name } : null,
      space: t.space ? { id: t.space.id } : null,
      url: t.url || null,
      custom_fields: extractCustomFields(t.custom_fields || []),
      checklists: checklists
    }
  });
}

/**
 * create_checklist: Create a checklist on a task.
 *
 * Params:
 *   task_id (required) - ClickUp task ID
 *   name (required) - checklist name
 */
function createChecklist(data) {
  if (!data.task_id) return jsonResponse({ error: "task_id required" });
  if (!data.name) return jsonResponse({ error: "name required" });

  var result = clickupRequest("POST", "/task/" + data.task_id + "/checklist", {
    name: data.name
  });

  return jsonResponse({
    success: true,
    task_id: data.task_id,
    checklist: result.checklist
  });
}

/**
 * add_checklist_items: Add items to an existing checklist.
 * ClickUp only supports adding one item per API call, so this loops through each item.
 *
 * Params:
 *   checklist_id (required) - ClickUp checklist ID
 *   items (required) - array of item name strings
 */
function addChecklistItems(data) {
  if (!data.checklist_id) return jsonResponse({ error: "checklist_id required" });
  if (!data.items || !data.items.length) return jsonResponse({ error: "items required (array of strings)" });

  var results = [];
  data.items.forEach(function(itemName) {
    try {
      var result = clickupRequest("POST", "/checklist/" + data.checklist_id + "/checklist_item", {
        name: itemName,
        assignee: null
      });
      results.push({ name: itemName, success: true, checklist_item: result.checklist_item || null });
    } catch (err) {
      results.push({ name: itemName, success: false, error: err.message });
    }
  });

  var successCount = results.filter(function(r) { return r.success; }).length;

  return jsonResponse({
    success: true,
    checklist_id: data.checklist_id,
    total: data.items.length,
    added: successCount,
    failed: data.items.length - successCount,
    results: results
  });
}

/**
 * create_checklist_with_items: Create a checklist on a task and add items in one call.
 *
 * Params:
 *   task_id (required) - ClickUp task ID
 *   name (required) - checklist name
 *   items (required) - array of item name strings
 */
function createChecklistWithItems(data) {
  if (!data.task_id) return jsonResponse({ error: "task_id required" });
  if (!data.name) return jsonResponse({ error: "name required" });
  if (!data.items || !data.items.length) return jsonResponse({ error: "items required (array of strings)" });

  // Step 1: Create the checklist
  var checklistResult = clickupRequest("POST", "/task/" + data.task_id + "/checklist", {
    name: data.name
  });

  var checklist = checklistResult.checklist;
  if (!checklist || !checklist.id) {
    return jsonResponse({ error: "Failed to create checklist", details: checklistResult });
  }

  // Step 2: Add each item to the checklist
  var itemResults = [];
  data.items.forEach(function(itemName) {
    try {
      var result = clickupRequest("POST", "/checklist/" + checklist.id + "/checklist_item", {
        name: itemName,
        assignee: null
      });
      itemResults.push({ name: itemName, success: true, checklist_item: result.checklist_item || null });
    } catch (err) {
      itemResults.push({ name: itemName, success: false, error: err.message });
    }
  });

  var successCount = itemResults.filter(function(r) { return r.success; }).length;

  return jsonResponse({
    success: true,
    task_id: data.task_id,
    checklist_id: checklist.id,
    checklist_name: data.name,
    total_items: data.items.length,
    items_added: successCount,
    items_failed: data.items.length - successCount,
    items: itemResults
  });
}

/**
 * update_checklist_item: Rename / resolve / reparent a single item.
 *
 * Accepts either direct IDs or name-based lookup (needs task_id to search).
 *
 * Params (direct):
 *   checklist_item_id (required if no name lookup)
 *   checklist_id (required if no name lookup)
 * Params (name-based lookup):
 *   task_id (required) - task to search
 *   checklist_name - name of the checklist (case-insensitive; omitted/ambiguous errors out unless only one checklist exists)
 *   item_name (required) - name of the item to update
 * Params (changes):
 *   name - new item name
 *   resolved - true/false to check/uncheck
 *   parent - parent item ID (null to clear)
 */
function updateChecklistItem(data) {
  var ids = resolveChecklistAndItem(data);
  if (ids.error) return jsonResponse({ error: ids.error });

  var payload = {};
  if (data.name !== undefined) payload.name = data.name;
  if (data.resolved !== undefined) {
    payload.resolved = (data.resolved === "true" || data.resolved === true);
  }
  if (data.parent !== undefined) {
    payload.parent = (data.parent === "null" || data.parent === null) ? null : data.parent;
  }

  if (Object.keys(payload).length === 0) {
    return jsonResponse({ error: "No fields to update. Provide name, resolved, or parent." });
  }

  clickupRequest("PUT",
    "/checklist/" + ids.checklist_id + "/checklist_item/" + ids.checklist_item_id,
    payload);

  return jsonResponse({
    success: true,
    checklist_id: ids.checklist_id,
    checklist_item_id: ids.checklist_item_id,
    updated: Object.keys(payload)
  });
}

/**
 * delete_checklist_item: Remove a single item from a checklist.
 *
 * Params: same lookup options as update_checklist_item.
 */
function deleteChecklistItem(data) {
  var ids = resolveChecklistAndItem(data);
  if (ids.error) return jsonResponse({ error: ids.error });

  clickupRequest("DELETE",
    "/checklist/" + ids.checklist_id + "/checklist_item/" + ids.checklist_item_id);

  return jsonResponse({
    success: true,
    checklist_id: ids.checklist_id,
    checklist_item_id: ids.checklist_item_id,
    deleted: true
  });
}

/**
 * edit_checklist: Rename or reorder an entire checklist.
 *
 * Params (direct):
 *   checklist_id (required if no name lookup)
 * Params (name-based lookup):
 *   task_id (required) - task to search
 *   checklist_name (required) - existing name of the checklist to edit
 * Params (changes):
 *   name - new checklist name
 *   position - numeric position
 */
function editChecklist(data) {
  var ids = resolveChecklist(data);
  if (ids.error) return jsonResponse({ error: ids.error });

  var payload = {};
  if (data.name !== undefined) payload.name = data.name;
  if (data.position !== undefined) payload.position = parseInt(data.position);

  if (Object.keys(payload).length === 0) {
    return jsonResponse({ error: "No fields to update. Provide name or position." });
  }

  clickupRequest("PUT", "/checklist/" + ids.checklist_id, payload);

  return jsonResponse({
    success: true,
    checklist_id: ids.checklist_id,
    updated: Object.keys(payload)
  });
}

/**
 * delete_checklist: Remove an entire checklist from a task.
 *
 * Params: same lookup options as edit_checklist.
 */
function deleteChecklist(data) {
  var ids = resolveChecklist(data);
  if (ids.error) return jsonResponse({ error: ids.error });

  clickupRequest("DELETE", "/checklist/" + ids.checklist_id);

  return jsonResponse({
    success: true,
    checklist_id: ids.checklist_id,
    deleted: true
  });
}

// --- Helpers ---

function parseDateToNoon(dateStr) {
  // Parse "YYYY-MM-DD" as noon UTC to prevent timezone rollback
  var d = new Date(dateStr);
  d.setUTCHours(12, 0, 0, 0);
  return d.getTime();
}

function resolveList(alias) {
  if (!alias) return null;
  return LISTS[alias] || alias;  // Fall through to raw ID if not an alias
}

function resolveFieldValue(fieldAlias, value) {
  // For method field: resolve alias to UUID
  if (fieldAlias === "method") {
    return METHOD_VALUES[value] || value;
  }

  // For checkbox fields: normalize to boolean
  if (fieldAlias === "this_week" || fieldAlias === "today") {
    if (value === "true" || value === true) return true;
    if (value === "false" || value === false) return false;
    return value;
  }

  return value;
}

/**
 * resolveChecklist: Return { checklist_id } from either direct id or name lookup.
 * Returns { error } on failure.
 */
function resolveChecklist(data) {
  if (data.checklist_id) {
    return { checklist_id: data.checklist_id };
  }

  if (!data.task_id) {
    return { error: "Provide checklist_id OR (task_id + checklist_name)" };
  }

  var t = clickupRequest("GET", "/task/" + data.task_id);
  var checklists = t.checklists || [];

  if (checklists.length === 0) {
    return { error: "Task has no checklists" };
  }

  // If only one checklist and no name given, use it
  if (!data.checklist_name) {
    if (checklists.length === 1) {
      return { checklist_id: checklists[0].id };
    }
    return { error: "Task has " + checklists.length + " checklists; provide checklist_name" };
  }

  var target = data.checklist_name.toLowerCase();
  var matches = checklists.filter(function(cl) {
    return cl.name.toLowerCase() === target;
  });

  if (matches.length === 0) {
    var names = checklists.map(function(cl) { return cl.name; }).join(", ");
    return { error: "No checklist named '" + data.checklist_name + "'. Found: " + names };
  }
  if (matches.length > 1) {
    return { error: "Ambiguous: " + matches.length + " checklists named '" + data.checklist_name + "'. Use checklist_id." };
  }

  return { checklist_id: matches[0].id };
}

/**
 * resolveChecklistAndItem: Return { checklist_id, checklist_item_id } from
 * either direct ids or name-based lookup.
 * Returns { error } on failure.
 */
function resolveChecklistAndItem(data) {
  // If both IDs are provided, use them directly.
  if (data.checklist_id && data.checklist_item_id) {
    return {
      checklist_id: data.checklist_id,
      checklist_item_id: data.checklist_item_id
    };
  }

  if (!data.task_id) {
    return { error: "Provide (checklist_id + checklist_item_id) OR (task_id + item_name + optional checklist_name)" };
  }
  if (!data.item_name) {
    return { error: "item_name required for name-based lookup" };
  }

  var t = clickupRequest("GET", "/task/" + data.task_id);
  var checklists = t.checklists || [];

  if (checklists.length === 0) {
    return { error: "Task has no checklists" };
  }

  // Narrow to a single checklist if name given
  var candidates = checklists;
  if (data.checklist_name) {
    var clTarget = data.checklist_name.toLowerCase();
    candidates = checklists.filter(function(cl) {
      return cl.name.toLowerCase() === clTarget;
    });
    if (candidates.length === 0) {
      var names = checklists.map(function(cl) { return cl.name; }).join(", ");
      return { error: "No checklist named '" + data.checklist_name + "'. Found: " + names };
    }
  }

  // Search for item by name across candidate checklists
  var itemTarget = data.item_name.toLowerCase();
  var hits = [];
  candidates.forEach(function(cl) {
    (cl.items || []).forEach(function(it) {
      if (it.name.toLowerCase() === itemTarget) {
        hits.push({ checklist_id: cl.id, checklist_item_id: it.id, checklist_name: cl.name });
      }
    });
  });

  if (hits.length === 0) {
    return { error: "No item named '" + data.item_name + "' found" +
      (data.checklist_name ? " in checklist '" + data.checklist_name + "'" : "") };
  }
  if (hits.length > 1) {
    var locs = hits.map(function(h) { return h.checklist_name; }).join(", ");
    return { error: "Ambiguous: item '" + data.item_name + "' exists in multiple checklists (" + locs + "). Pass checklist_name to disambiguate." };
  }

  return {
    checklist_id: hits[0].checklist_id,
    checklist_item_id: hits[0].checklist_item_id
  };
}

function extractCustomFields(fields) {
  var result = {};
  fields.forEach(function(cf) {
    // Only include the fields we care about
    var alias = null;
    for (var key in FIELDS) {
      if (FIELDS[key] === cf.id) {
        alias = key;
        break;
      }
    }
    if (alias) {
      if (alias === "this_week" || alias === "today") {
        result[alias] = cf.value === "true" || cf.value === true;
      } else if (alias === "method") {
        // Reverse-lookup method UUID to alias
        var methodAlias = null;
        if (cf.value !== undefined && cf.value !== null) {
          for (var mk in METHOD_VALUES) {
            if (METHOD_VALUES[mk] === String(cf.value)) {
              methodAlias = mk;
              break;
            }
          }
        }
        // Fallback: check type_config options by orderindex
        if (!methodAlias && cf.type_config && cf.type_config.options && cf.value !== undefined) {
          var selectedOption = cf.type_config.options.find(function(opt) {
            return String(opt.orderindex) === String(cf.value);
          });
          if (selectedOption) methodAlias = selectedOption.name;
        }
        result[alias] = methodAlias || cf.value;
      } else {
        result[alias] = cf.value;
      }
    }
  });
  return result;
}

function clickupRequest(method, endpoint, payload) {
  var token = PropertiesService.getScriptProperties().getProperty("CLICKUP_TOKEN");
  if (!token) {
    throw new Error("CLICKUP_TOKEN not set in Script Properties. Go to Project Settings > Script Properties and add it.");
  }

  var url = CLICKUP_BASE + endpoint;
  var options = {
    method: method.toLowerCase(),
    headers: {
      "Authorization": token,
      "Content-Type": "application/json"
    },
    muteHttpExceptions: true
  };

  if (payload && (method === "POST" || method === "PUT")) {
    options.payload = JSON.stringify(payload);
  }

  var response = UrlFetchApp.fetch(url, options);
  var code = response.getResponseCode();
  var body = response.getContentText();

  if (code >= 400) {
    throw new Error("ClickUp API error " + code + ": " + body);
  }

  return body ? JSON.parse(body) : {};
}

function jsonResponse(data) {
  return ContentService
    .createTextOutput(JSON.stringify(data))
    .setMimeType(ContentService.MimeType.JSON);
}
