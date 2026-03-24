---
name: notion-to-csv
description: "Convert Notion dialogue card pages into CSV files for Unity import. Use this skill whenever the user mentions converting cards, exporting cards to CSV, importing cards to Unity, or wants to move dialogue from Notion into the game. Also trigger when the user says things like 'convert my cards', 'export the database', 'make a CSV', or 'I'm ready to import'."
---

# Notion to CSV Converter for Crush Dialogue Cards

This skill converts dialogue card pages from the Notion "Dialogue Cards" database into CSV files that the Unity `DialogueCardImporter` can parse.

## The Notion Database

Database URL: https://www.notion.so/c0b3a0d8f2fc4952abc6dd9079e04d66
Data source ID: `fc9d5b9e-7dff-48ed-91cd-0d495a85f339`

Each row is a card with properties:
- **Card Name** (title) — used as the asset filename in Unity
- **Status** — Draft, Ready, Imported, Needs Revision

The card's branching structure lives in the **page body**, not in database properties.

## Page Body Structure

Each card page follows this layout (some sections may be absent):

```
Preview Text: <text> | Cost: <number>

#### Death (toggle)
    Luke dialogue table (Boy/BoyInternal rows)

#### Awkward (toggle) — OPTIONAL, not all cards have this
    Luke dialogue table
    Charm Impact table (Low | Neutral | Positive | High values)
    Daisy state labels + dialogue tables with Conf column

#### Normal (toggle)
    Luke dialogue table
    Charm Impact table (Low | Neutral | Positive | High values)
    Daisy state labels + dialogue tables with Conf column
```

## How to Parse a Card Page

Use `notion-fetch` to get the page content, then extract:

### 1. Card-level info
From the first line of content: `**Preview Text:** <text> \| **Cost:** <number>`

### 2. Luke branches
Each `#### <BranchName> {toggle="true"}` heading is a Luke branch. The branch name is "Death", "Awkward", or "Normal".

Inside each toggle, the first table contains Luke's dialogue lines. Each row has:
- Column 1: Character name (Boy, Girl, or BoyInternal)
- Column 2: Dialogue text

### 3. Charm Impact (inside Awkward/Normal toggles only)
A table with header row containing charm state names (Low, Neutral, Positive, High) and a data row with integer impact values. Death branch has no charm impact.

### 4. Daisy branches (inside Awkward/Normal toggles only)
After the charm impact table, there are labeled sections for each Daisy charm state. The label is a `<span>` or plain text with the state name (Death, Negative/Low, Neutral, Positive, High). Each is followed by a table with:
- Column 1: Character (usually Girl, sometimes BoyInternal)
- Column 2: Dialogue text
- Column 3: Confidence impact (integer, may be empty)

### Name Mapping
The user may use "Negative" in Notion for what Unity calls "Low". Always map:
- "Negative" → "Low"

### Combined States
The user may label a Daisy branch like "Neutral & Positive" meaning the same dialogue applies to both states. When you see this, output two separate Daisy branch entries in the CSV with identical dialogue.

## CSV Output Format

The CSV must match what `DialogueCardImporter.cs` expects. The format is vertical/hierarchical with 5 columns (A through E):

```
Card Name,<name>,,,
Preview Text,<text>,,,
Cost,<number>,,,
,Luke,<BranchName>,,
,,<Character>,<dialogue text>,
,,<Character>,<dialogue text>,
,CharmImpact,<State>,<impact>,
,CharmImpact,<State>,<impact>,
,Daisy,<State>,,
,,<Character>,<dialogue text>,<confidence impact>
,,<Character>,<dialogue text>,<confidence impact>
```

Key rules:
- Column A only has content for card-level labels (Card Name, Preview Text, Cost)
- Column B has section markers: "Luke", "CharmImpact", "Daisy"
- Luke lines: C=Character, D=dialogue text
- CharmImpact: C=state name (Low/Neutral/Positive/High), D=impact value
- Daisy lines: C=Character, D=dialogue text, E=confidence impact
- Dialogue containing commas must be wrapped in quotes
- CharmImpact and Daisy sections nest under the Luke branch that precedes them
- Death branch typically has no CharmImpact or Daisy sections

## Workflow

1. **Fetch cards** — Either fetch specific pages by URL, or search the database for cards with a specific Status (e.g., "Ready")
2. **Parse each page** — Extract the structure described above
3. **Generate CSV** — One CSV file containing all cards, saved to `Assets/` in the Unity project at `/sessions/jolly-stoic-faraday/mnt/Crush/Assets/`
4. **Share the file** — Give the user a link to the CSV
5. **Update status** — Optionally update each converted card's Status to "Imported"

When the user says something vague like "convert my cards", ask if they want all cards, just "Ready" ones, or specific cards by name.
