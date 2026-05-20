**Format notes (skip on read-through, useful for reference):**

- Each card is the entry of one conversation thread Luke can pick during a Conversation phase.
- **Preview** = bubble text shown on the draft button.
- **Cost** = confidence cost to play the card.
- **Luke branches** are chosen by Luke's confidence level when the card plays:
  - *Death* (confidence ≤ 0)
  - *Awkward* (confidence 1–3) — used only if the card defines an Awkward branch; otherwise falls back to Normal.
  - *Normal* (confidence 4+)
- **Charm shift** is how Daisy's charm meter moves when Luke plays this branch. It's keyed off Daisy's pre-line charm state.
- **Daisy responses** are then selected by Daisy's charm state **after** the shift:
  - *Death* (0), *Low* (1–2), *Neutral* (3–5), *Positive* (6–8), *High* (9–10)
- Inline `[+N conf]` / `[-N conf]` annotations on a line = that line's confidence impact. Annotations omitted when impact is 0.
- `(internal)` prefix = Luke's internal monologue, not spoken aloud.
- `Upgrades:` block references upgrade SOs whose content lives in the Upgrade doc.

---

# CARD: Book

**Preview:** Book?
**Cost:** 3

## Death branch (Luke conf ≤ 0)

Luke: What book are you…….

*(no Daisy response — Luke dies mid-line)*

## Normal branch (Luke conf 4+)

Luke: What book is that you're reading?

**Charm shift from Luke's line:**
- Low → +2 charm
- Neutral → +2 charm
- Positive → 0 charm
- High → −2 charm

**Daisy responses:**

**Death** (charm 0)
*(no response)*

**Low** (charm 1–2)
Daisy `[+1 conf]`: Oh this? It's ummm this fantasy series.
Daisy `[+1 conf]`: This is the fourth one.
Daisy `[+1 conf]`: They're just okay.

**Neutral** (charm 3–5)
Daisy `[+1 conf]`: It's a fantasy series I've been reading for a while.
Daisy `[+1 conf]`: This is actually the fourth one.
Daisy `[+2 conf]`: It's kinda dorky though….

**Positive** (charm 6–8)
Daisy `[+1 conf]`: It's just a fantasy series I've been reading for a while.
Daisy `[+1 conf]`: This is actually the fourth one.
Daisy `[+3 conf]`: It's dorky… but I like it.
*(internal)* Luke: God she's perfect

**High** (charm 9–10)
Daisy `[-1 conf]`: Oh it's nothing.
Daisy: Just this fantasy series I've been reading.

## Draft lines (Luke's pre-play thoughts)

- She's holding that book....
- She might like if I ask her about it.

## Upgrades

- Available upgrades: *(none)*
- Upgrade threshold: 3

---

# CARD: Class

**Preview:** Class was boring
**Cost:** 1

## Death branch (Luke conf ≤ 0)

Luke: Class was uhhh… pretty….

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: Class was pretty boring huh?
Luke: I was fighting to stay awake when Mr. Miller started talking about the Roman Empire.

**Charm shift from Luke's line:**
- Low → +2 charm
- Neutral → +3 charm
- Positive → +1 charm
- High → 0 charm

**Daisy responses:**

**Death** (charm 0)
Daisy: yeah that class sucked gotta go see ya later

**Low** (charm 1–2)
Daisy: haha yeah. Not exactly the most exciting class I've ever been to.

**Neutral** (charm 3–5)
Daisy `[+2 conf]`: Seriously haha. Ugh 5th period is the absolute worst.

**Positive** (charm 6–8)
Daisy `[+1 conf]`: I know right. It's like all that guy thinks about is the Roman Empire haha.
Daisy `[+2 conf]`: He needs some new hobbies.

**High** (charm 9–10)
Daisy `[+2 conf]`: HAHAH right?! He is so boring I want to DIE every time 5th period rolls around.
Daisy `[+2 conf]`: HAHA you're hilarious!

## Draft lines

- Something easy
- Just talk about how boring 4th period was

## Upgrades

- Available upgrades: *(none)*
- Upgrade threshold: 3

---

# CARD: Dance ⭐ *(special end-game card — isDance: true)*

**Preview:** DANCE?
**Cost:** 0

## Default branch (single branch, no Awkward/Death/Normal split)

*(internal)* Luke: Here goes nothing
Luke `[-1 conf]`: I was- umm.... wondering... if.....
Luke `[-1 conf]`: uhhh... if you would... uhh- maybe
Luke `[-1 conf]`: be interested.....
Luke `[-1 conf]`: in go- uhhh.....
Luke `[-1 conf]`: sorry... ummm- do you wanna.....
Luke `[-2 conf]`: go to the dance....
Luke `[-3 conf]`: with me?

*(no charm shift defined; no Daisy response branch — handled by AskedToDance success path)*

## Draft lines

*(empty)*

---

# CARD: Look Nice

**Preview:** Look Nice
**Cost:** 6

## Death branch (Luke conf ≤ 0)

Luke: You are soooo hot

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: You uhhh…. You look really nice today.

**Charm shift from Luke's line:**
- Low → −3 charm
- Neutral → −1 charm
- Positive → +3 charm
- High → 0 charm

**Daisy responses:**

**Death** (charm 0)
Daisy `[-6 conf]`: Ohh - ummm…. thanks? I guess…

**Low** (charm 1–2)
Daisy `[-1 conf]`: Ohh - ummm…. thanks.

**Neutral** (charm 3–5)
Daisy: Thanks… that's nice of you.

**Positive** (charm 6–8)
Daisy `[+3 conf]`: Aww thanks!
Daisy `[+1 conf]`: That umm… means a lot

**High** (charm 9–10)
Daisy: Really?! You think so???
Daisy `[+1 conf]`: I mean - thanks! That's really nice.
Daisy `[+4 conf]`: You look go- uhh… nice.. too

## Draft lines

- Girls like it when you compliment them
- I think

---

# CARD: Math Test

**Preview:** Math Test
**Cost:** 2

## Death branch (Luke conf ≤ 0)

Luke: So- uhhhhh…. How did that math te-

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: How did you do on Ms. Parsons test?
Luke: I had soccer practice last night so I barely got to study.

**Charm shift from Luke's line:**
- Low → 0 charm
- Neutral → 0 charm
- Positive → 0 charm
- High → −2 charm

**Daisy responses:**

**Death** (charm 0)
Daisy: walks away

**Low** (charm 1–2)
Daisy `[-1 conf]`: uhhh... it was okay

**Neutral** (charm 3–5)
Daisy: I think I did okay. Same, I had volleyball practice and didn't get home until like 8.
Daisy `[+3 conf]`: I started studying before even taking a shower. I was so gross.

**Positive** (charm 6–8)
Daisy `[+2 conf]`: It was pretty good! I didn't know you still played soccer. That's awesome!

**High** (charm 9–10)
Daisy `[-1 conf]`: Oh.. the test. Ummm, I think I did pretty good. I wasn't able to study much either

## Draft lines

- She probably got an A on Ms. Parsons test
- She's perfect

---

# CARD: Movies

**Preview:** Movies
**Cost:** 2

## Death branch (Luke conf ≤ 0)

Luke: So…. ummmm……
Luke: Seen any good mov-

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: Have you watched any good movies lately?
Luke: I haven't seen anything in a while. I could use a good recommendation.

**Charm shift from Luke's line:**
- Low → +1 charm
- Neutral → +2 charm
- Positive → +1 charm
- High → −1 charm

**Daisy responses:**

**Death** (charm 0)
*(no response)*

**Low** (charm 1–2)
Daisy `[-2 conf]`: Nooo…. not that I can remember.

**Neutral** (charm 3–5)
Daisy `[+1 conf]`: Yeah I did actually!
Daisy `[+1 conf]`: It was this really old movie about these two cops.
Daisy: It was pretty funny.

**Positive** (charm 6–8)
Daisy `[+1 conf]`: Yeah I did actually!
Daisy `[+1 conf]`: It was this really old movie about these two cops.
Daisy `[+1 conf]`: It was soooo funnyyyy!
Daisy `[+1 conf]`: You need to see it!

**High** (charm 9–10)
Daisy `[+1 conf]`: Yeah I did actually!
Daisy `[+1 conf]`: It was this really old movie about these two cops.
Daisy `[+1 conf]`: It was soooo funnyyyy!
Daisy `[+3 conf]`: We should watch it together!
Daisy: I mean - if you want to…

## Draft lines

- I think she likes movies....

---

# CARD: Music

**Preview:** Music
**Cost:** 3

## Death branch (Luke conf ≤ 0)

Luke: Any, uhhhh…….
Luke: good musi-

*(no Daisy response)*

## Awkward branch (Luke conf 1–3)

Luke: So have you listened to that Farthing Penny song yet?

**Charm shift from Luke's line:**
- Low → −3 charm
- Neutral → −1 charm
- Positive → −2 charm
- High → −4 charm

**Daisy responses:**

**Death** (charm 0)
Daisy: HAHA Farthing Penny? You mean Penny Farthing? Some fan you are hahaha

**Low** (charm 1–2)
Daisy `[-1 conf]`: HAHA that's not their name. It's Penny Farthing. But yeah I love that song.

**Neutral** (charm 3–5)
Daisy: I think you mean Penny Farthing. But of course I've listened to it!

**Positive** (charm 6–8)
Daisy `[+1 conf]`: Ohhh Luke…. It's Penny Farthing. And duhhhh I've listened to it like a thousand times already.

**High** (charm 9–10)
Daisy `[+2 conf]`: Uhhh…. they're called Penny Farthing, not Farthing Penny. But close enough. Don't worry, I won't tell anyone.

## Normal branch (Luke conf 4+)

Luke: Have you heard that new Penny Farthing song yet?

**Charm shift from Luke's line:**
- Low → +4 charm
- Neutral → +3 charm
- Positive → +2 charm
- High → +1 charm

**Daisy responses:**

**Death** (charm 0)
*(no response)*

**Low** (charm 1–2)
Daisy `[+3 conf]`: Oh you like them too?? Yeah it's really good. I've been playing it a bunch

**Neutral** (charm 3–5)
Daisy `[+4 conf]`: It's soooo good! OMG I didn't know you liked them.

**Positive** (charm 6–8)
Daisy `[+2 conf]`: OMG I'm obsessed with the new song. It has like 2 million plays already.
Daisy `[+2 conf]`: but at least a million are me.

**High** (charm 9–10)
Daisy `[+2 conf]`: They're so good. I totally knew about them like 3 years ago before they blew up.
Daisy `[+4 conf]`: That's cool you like them too! We should listen to the new album together

## Draft lines

- What's that band she loves again?
- Penny Farthing??
- Or is it Farthing Penny...?

## Upgrades

- Available upgrades: Music_CostUpgrade
- Upgrade threshold: 1

---

# CARD: Notes

**Preview:** I took notes
**Cost:** 1

## Death branch (Luke conf ≤ 0)

Luke: Do you want my n-

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: You were out on Monday right? I took notes in Ms. Greene's class if you need them.

**Charm shift from Luke's line:**
- Low → +2 charm
- Neutral → +2 charm
- Positive → 0 charm
- High → −2 charm

**Daisy responses:**

**Death** (charm 0)
*(no response)*

**Low** (charm 1–2)
Daisy `[-1 conf]`: Oh thanks. I'll uhhh…. let you know if I need them.

**Neutral** (charm 3–5)
Daisy `[+2 conf]`: Thank you! I've been meaning to find someone to get those notes from.
Daisy: That would be really helpful.

**Positive** (charm 6–8)
Daisy `[+2 conf]`: You're the best! That would be awesome! You are such a life saver.

**High** (charm 9–10)
Daisy `[+2 conf]`: You're the best! That would be awesome! You are such a life saver.

## Draft lines

*(none)*

---

# CARD: Soccer ⚠ *(old schema — see note)*

**Preview:** Soccer
**Cost:** 4

> **⚠ Schema note:** Soccer's SO uses the older shape where `daisyBranches` and `charmImpacts` sit at the card level (not per-Luke-branch), and branches use a different `branchName` / `charmState` keying scheme. Including content as-authored. Flag for migration to current schema or design call on whether this older shape should stay supported.

## Luke branches

**Afford** (charmState 0, conf 0–45)
Luke: I have a soccer game on Saturday.
Luke: It's actually the championship game.

**Death** (charmState 0, conf 0–45)
Luke: I'm in the championship match on Sa...-

## Charm shift (card-level)

- Low → 0 charm
- Neutral → +1 charm
- Positive → +2 charm
- High → +3 charm

## Daisy responses (card-level, charm-state-keyed)

**Death** (charm 0) — branch "Death"
Daisy: Oh, nice. I gotta go to class now. See you later.

**Low** (charm 1–2) — branch "Low"
Daisy `[-1 conf]`: Oh... you play soccer? That's pretty cool.

**Neutral** (charm 3–5) — branch "Neutral"
Daisy `[+1 conf]`: I forgot you play soccer. That's really cool though!

**Positive** (charm 6–8) — branch "Positive"
Daisy `[+4 conf]`: That is so cool. Good luck. Score a goal for me :)

**High** (charm 9–10) — branch "High"
Daisy `[+4 conf]`: OMG you're definitely gonna win. That is so cool. I wish I could be there.

## Draft lines

*(none)*

---

# CARD: Weekend

**Preview:** Weekend
**Cost:** 3

## Death branch (Luke conf ≤ 0)

Luke: Soooo…. what are you up t-

*(no Daisy response)*

## Normal branch (Luke conf 4+)

Luke: So.. do you..umm.. have any plans this weekend?

**Charm shift from Luke's line:**
- Low → −2 charm
- Neutral → +1 charm
- Positive → +2 charm
- High → 0 charm

**Daisy responses:**

**Death** (charm 0)
Daisy: Ummm…. no - nott really….

**Low** (charm 1–2)
Daisy: Ohh you know - just the usual stuff

**Neutral** (charm 3–5)
Daisy `[+2 conf]`: Not too much. My little brother turns 10 on Saturday so he's having a birthday party.
Daisy `[-1 conf]`: It's Star Wars themed, I think. Or something nerdy like that haha.

**Positive** (charm 6–8)
Daisy: Ugh I wish. My parents are throwing this big 10th birthday party for my little brother.
Daisy `[+3 conf]`: They said I have to be there.

**High** (charm 9–10)
Daisy: Ugh I wish. My parents are throwing this big 10th birthday party for my little brother.
Daisy `[+3 conf]`: They said I have to be there.
Daisy `[+3 conf]`: But that's it though! I'm totally free outside of that.

## Draft lines

- That's what guys with girlfriends talk about.
- right?
- Their big plans for the weekend?

---

# Open questions / gaps Davis should weigh in on

1. **Soccer's old schema.** Either migrate Soccer to the current per-Luke-branch shape, or expand the doc format to support both (probably not worth it).
2. **Tags.** Class (Neutral/Positive/High daisy branches) and Book (Neutral/Positive/High daisy branches) reference DialogueTag enum values in the SO. Where should tags be authored — in the cards doc inline, in a tags-only doc, or stay programmer-set in the inspector?
3. **Per-card line annotations omitted by default when 0.** Worth reconsidering — maybe writers want to see `[0 conf]` explicitly to know nothing's "missing"?
4. **Draft lines under Movies, Notes, Soccer.** Movies has only 1 line; Notes and Soccer have none. Confirm these are deliberate, not unfilled.
5. **Dance card's "Default" branch name.** Doesn't follow Death/Awkward/Normal — should the schema be extended with a fourth "Scripted" branch type, or is Dance always going to be one-off?
6. **No `isDance` / `revealed` / `buttonColor` fields exposed in this doc** — assumed to be programmer-side metadata, not writer-facing. Confirm.
