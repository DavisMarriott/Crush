import { useState } from "react";

const tasks = [
  {
    id: "86b9e1qh9",
    name: "Central phase manager / state machine",
    list: "Base Loop",
    status: "in progress",
    url: "https://app.clickup.com/t/86b9e1qh9",
    description: "Single source of truth for \"what phase am I in right now\" with explicit transitions. Enables other systems to react to phase changes via event or query.",
    checklist: null,
    notes: "Singleton + event built. Wiring remaining transitions incrementally."
  },
  {
    id: "86b9e1qen",
    name: "Build Reflect phase — v1 / foundation",
    list: "Base Loop",
    status: "complete",
    url: "https://app.clickup.com/t/86b9e1qen",
    description: "Reflect phase exists as a distinct loop moment after Death and before Upgrade/Draft. Bare-bones data layer so other systems can key off \"I'm in Reflect\".",
    checklist: null,
    notes: "Reflect beat wired into DeathRespawn.Death() with phase transitions."
  },
  {
    id: "86b9e1t1v",
    name: "Death → Reflect handoff",
    list: "Death",
    status: "complete",
    url: "https://app.clickup.com/t/86b9e1t1v",
    description: "State transition from Death into Reflect. Defines what data travels across the boundary: last conversation outcome, cards played, flags, loop number.",
    checklist: null,
    notes: "LoopSnapshot captures confidence, charm, death cause, cards played/unplayed."
  },
  {
    id: "86b9e1qmr",
    name: "Loop counter & cross-loop meta-state tracking",
    list: "Base Loop",
    status: "to do",
    url: "https://app.clickup.com/t/86b9e1qmr",
    description: "The data layer Reflect and other systems key off to vary content based on what's happened across loops.",
    checklist: {
      name: "Tracks",
      items: [
        { text: "Loop number (how many attempts)", done: false },
        { text: "Last conversation outcome", done: false },
        { text: "Cards played last loop", done: false },
        { text: "Any other flags Reflect/dynamic hallway/milestone logic needs", done: false }
      ]
    },
    notes: null
  },
  {
    id: "86b9epgbz",
    name: "Character upgrade system (v1) — Approach Confidence",
    list: "Base Loop",
    status: "to do",
    url: "https://app.clickup.com/t/86b9epgbz",
    description: "Reusable Character Upgrade system v1, anchored by the Approach Confidence upgrade. Unlocked via Reflection self-talk on loop 5.",
    checklist: {
      name: "Checklist",
      items: [
        { text: "Define CharacterUpgrade data shape (SO)", done: false },
        { text: "Upgrade registry + \"is unlocked\" query", done: false },
        { text: "Unlock trigger API callable from Reflect phase", done: false },
        { text: "Condition evaluator with loop-count support", done: false },
        { text: "Implement Approach Confidence upgrade (pick effect)", done: false },
        { text: "Write the paired reflection self-talk line", done: false },
        { text: "Wire Reflect → self-talk line → unlock on loop 5", done: false },
        { text: "Verify effect applies in next Hallway/Conversation loop", done: false }
      ]
    },
    notes: null
  }
];

const statusConfig = {
  "in progress": { label: "In Progress", color: "#E07A2F", bg: "#E07A2F" },
  "to do": { label: "To Do", color: "#9B9B9B", bg: "#9B9B9B" },
  "complete": { label: "Complete", color: "#6B9E78", bg: "#6B9E78" }
};

const statusOrder = { "in progress": 0, "to do": 1, "complete": 2 };

function TaskCard({ task, isExpanded, onToggle }) {
  const doneCount = task.checklist?.items.filter(i => i.done).length || 0;
  const totalCount = task.checklist?.items.length || 0;
  const config = statusConfig[task.status];
  const isComplete = task.status === "complete";

  return (
    <div
      style={{
        borderRadius: 10,
        padding: "14px 16px",
        marginBottom: 6,
        background: isComplete ? "#F5F0EB" : "#FFFFFF",
        opacity: isComplete ? 0.55 : 1,
        cursor: "pointer",
        transition: "all 0.15s ease",
        boxShadow: isComplete ? "none" : "0 1px 3px rgba(0,0,0,0.04)"
      }}
      onClick={onToggle}
    >
      <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
        <div
          style={{
            width: 7,
            height: 7,
            borderRadius: "50%",
            background: config.bg,
            flexShrink: 0,
            opacity: 0.85
          }}
        />
        <div style={{ flex: 1 }}>
          <div style={{
            fontSize: 13.5,
            fontWeight: 500,
            color: isComplete ? "#A09080" : "#3D3229",
            textDecoration: isComplete ? "line-through" : "none",
            letterSpacing: -0.1
          }}>
            {task.name}
          </div>
          <div style={{ fontSize: 11, color: "#B0A898", marginTop: 2, display: "flex", gap: 10, alignItems: "center" }}>
            <span>{task.list}</span>
            {task.checklist && (
              <span style={{
                color: doneCount === totalCount && totalCount > 0 ? "#6B9E78" : "#C4B8A8",
                fontWeight: 500,
                fontSize: 10.5
              }}>
                {doneCount}/{totalCount}
              </span>
            )}
          </div>
        </div>
        <span style={{
          color: "#C4B8A8",
          fontSize: 9,
          transform: isExpanded ? "rotate(180deg)" : "rotate(0deg)",
          transition: "transform 0.15s"
        }}>
          ▾
        </span>
      </div>

      {isExpanded && (
        <div style={{ marginTop: 12, paddingLeft: 17, borderTop: "1px solid #EDE8E2", paddingTop: 10 }}>
          <p style={{ fontSize: 12, color: "#8A7E70", lineHeight: 1.6, margin: "0 0 8px 0" }}>
            {task.description}
          </p>

          {task.notes && (
            <div style={{
              fontSize: 11,
              color: "#C4793C",
              background: "#FDF5ED",
              padding: "7px 10px",
              borderRadius: 6,
              marginBottom: 8,
              lineHeight: 1.4,
              borderLeft: "2px solid #E8AA6A"
            }}>
              {task.notes}
            </div>
          )}

          {task.checklist && (
            <div style={{ marginTop: 6 }}>
              <div style={{ fontSize: 10, color: "#B0A898", marginBottom: 5, fontWeight: 600, textTransform: "uppercase", letterSpacing: 0.4 }}>
                {task.checklist.name}
              </div>
              {task.checklist.items.map((item, i) => (
                <div key={i} style={{
                  fontSize: 12,
                  color: item.done ? "#C4B8A8" : "#5C5044",
                  padding: "3px 0",
                  display: "flex",
                  gap: 7,
                  alignItems: "flex-start",
                  lineHeight: 1.4
                }}>
                  <span style={{ color: item.done ? "#6B9E78" : "#D4C8B8", fontSize: 13, lineHeight: 1.3 }}>
                    {item.done ? "●" : "○"}
                  </span>
                  <span style={{ textDecoration: item.done ? "line-through" : "none" }}>{item.text}</span>
                </div>
              ))}
            </div>
          )}

          <a
            href={task.url}
            target="_blank"
            rel="noopener noreferrer"
            onClick={e => e.stopPropagation()}
            style={{
              fontSize: 10.5,
              color: "#C4793C",
              textDecoration: "none",
              marginTop: 10,
              display: "inline-block",
              fontWeight: 500,
              letterSpacing: 0.2
            }}
          >
            Open in ClickUp →
          </a>
        </div>
      )}
    </div>
  );
}

export default function TodayBoard() {
  const [expanded, setExpanded] = useState(null);

  const inProgress = tasks.filter(t => t.status === "in progress");
  const toDo = tasks.filter(t => t.status === "to do");
  const complete = tasks.filter(t => t.status === "complete");

  return (
    <div style={{
      maxWidth: 480,
      margin: "0 auto",
      padding: "24px 20px",
      fontFamily: "'Styrene A', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
      background: "#F9F5F0",
      minHeight: "100vh"
    }}>
      <div style={{ marginBottom: 22 }}>
        <div style={{ fontSize: 11, fontWeight: 600, color: "#E07A2F", textTransform: "uppercase", letterSpacing: 1.2, marginBottom: 4 }}>
          Crush
        </div>
        <h2 style={{ margin: 0, fontSize: 20, fontWeight: 600, color: "#3D3229", letterSpacing: -0.3 }}>
          Today
        </h2>
        <div style={{ fontSize: 11.5, color: "#B0A898", marginTop: 3 }}>
          {complete.length} of {tasks.length} complete
        </div>
      </div>

      {inProgress.length > 0 && (
        <div style={{ marginBottom: 18 }}>
          <div style={{
            fontSize: 10,
            fontWeight: 600,
            color: "#E07A2F",
            textTransform: "uppercase",
            letterSpacing: 0.8,
            marginBottom: 7,
            paddingLeft: 2
          }}>
            In Progress
          </div>
          {inProgress.map(t => (
            <TaskCard key={t.id} task={t} isExpanded={expanded === t.id} onToggle={() => setExpanded(expanded === t.id ? null : t.id)} />
          ))}
        </div>
      )}

      {toDo.length > 0 && (
        <div style={{ marginBottom: 18 }}>
          <div style={{
            fontSize: 10,
            fontWeight: 600,
            color: "#B0A898",
            textTransform: "uppercase",
            letterSpacing: 0.8,
            marginBottom: 7,
            paddingLeft: 2
          }}>
            To Do
          </div>
          {toDo.map(t => (
            <TaskCard key={t.id} task={t} isExpanded={expanded === t.id} onToggle={() => setExpanded(expanded === t.id ? null : t.id)} />
          ))}
        </div>
      )}

      {complete.length > 0 && (
        <div style={{ marginBottom: 18 }}>
          <div style={{
            fontSize: 10,
            fontWeight: 600,
            color: "#6B9E78",
            textTransform: "uppercase",
            letterSpacing: 0.8,
            marginBottom: 7,
            paddingLeft: 2
          }}>
            Complete
          </div>
          {complete.map(t => (
            <TaskCard key={t.id} task={t} isExpanded={expanded === t.id} onToggle={() => setExpanded(expanded === t.id ? null : t.id)} />
          ))}
        </div>
      )}
    </div>
  );
}