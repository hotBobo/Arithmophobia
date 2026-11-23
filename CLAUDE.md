# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## How to work with the user

The user is a student learning game design and C++, and is new to C# and Unity. This project is how they are learning, so the goal is their understanding, not just working code.

- Do not write or edit game code (anything under `Assets/Scripts/`, `Assets/Tests/`, scenes, or prefabs) unless the user explicitly asks you to.
- Instead, guide them. Explain how the existing code works, point to the relevant files and lines, introduce the C# or Unity concept involved (comparing it to C++ where that helps), and suggest the next step so they can write it themselves.
- When reviewing their code, explain what is wrong and why, and let them make the fix.
- Editing documentation such as this file is fine when asked.

## Project

Arithmophobia is a Unity game (Unity 6, editor version `6000.5.9f1`) using URP 17.6 and the new Input System (`com.unity.inputsystem`). The project is early-stage: there is one scene (`Assets/Scenes/SampleScene.unity`) with a playable prototype round. It has a working equation evaluator, a weighted `MathPool`, a `KeyHand` of spendable keys, a `Round` tracking target and submissions, and a `KeypadController` that ties them to hand-placed TextMeshPro buttons. Tests exist for the evaluator and `MathPool`. Roadmap Phase 1 (math engine) is mostly done, Phase 2 (UI) is a prototype, and Phase 3 (GPA, shop) has not started.

## Game design (planned, mostly not yet implemented)

A student who struggles with math makes a deal to pass their exams. The presentation is a handwritten test booklet, with equations written on notebook paper. Upgrades are bought from an "under the desk" pencil-case shop.

**Core loop**
- Each question starts with a keypad grid of operands (digits, constants such as π and e, special tiles such as negative numbers) and operators.
- The player builds an equation from the available keys and submits it. It is evaluated with standard PEMDAS.
- A key can be pressed once per equation. Pressing it spends it (the button greys out). On a successful submit, spent keys morph into new keys drawn at random from the player's pool, and unused keys stay on the keypad.
- A failed equation (syntax error or divide by zero) does not use up a submission, and the spent keys are restored instead of redrawn.
- `ANS` carries the previous answer into the next equation, so answers can scale exponentially.

**Round rules**
- Answers do not accumulate into a score. Each round has a target, and only the **last** answer (the current `ANS`) counts. That is the risk: a later, worse answer replaces a better one.
- Each round has a limited number of submissions. Reaching the target does not end the round.
- The round ends when the player presses **Turn In**, or automatically when submissions run out, using the current answer. It is a win if that answer is at or above the target, and a loss otherwise.
- The player chooses between turning in a safe answer and spending more submissions to aim higher.
- Open questions: whether Turn In is allowed below the target (counts as a loss) or disabled until the target is reached, and whether "exact" uses a small tolerance (it should, since the evaluator works in `double`).
- Not yet implemented: `Round.cs` still ends the round as soon as an answer reaches the target, and has no Turn In. It needs reworking to match the rules above.

**Pools**
- The operand pool is a custom bag of digits, constants, and special tiles.
- The operator pool has basic operators (`+`, `-`) with high drop rates. Exotic operators (`×`, `÷`, `x^y`, `!`) are unlocked in the shop.
- Draws are weighted random selections.

**GPA economy**
- GPA is capped at 4.00. Falling below 2.00 means Academic Probation, which is game over.
- Winning a round earns a base GPA that depends on the round type: +0.20 (small), +0.30 (big), +0.40 (boss).
- Bonuses: +0.50 for an exact match on the target, and +0.10 for each submission left unused at turn-in. More bonus ideas are being considered, and the numbers are placeholders to tune. Keep them as named constants.
- GPA calculation lives outside `Round`. `Round` only reports the facts (final answer, target, submissions left, status).
- Shop purchases cost GPA (for example −0.40). Purchases include permanent keypad hardware upgrades, desk items, and deck-thinning ("White-Out").

**Test structure**
- Page 1 (Short Answer), then the Desk Shop, then Page 2 (Word Problem), then the Desk Shop, then Page 3 (Long Answer Boss).
- Boss quizzes apply modifiers such as "No Even Answers Allowed", "Fractions Inverted", and "Exact Target Match Only".

**Roadmap**
1. **Math engine (pure C#, no UI).**
   - Equation evaluator: done (no unary minus yet), see below.
   - ANS state tracking: done, held in `Round.CurrentAns`.
   - `MathPool` with weighted random selection: done, with tests.
   - `KeyHand` (spendable key slots drawn from the pools): done, no tests yet.
   - `Round` (target, submissions, status): started. Needs the Turn In rule and tests.
2. **UI and input.**
   - Keypad: `KeypadController` handles operand, operator, `ANS`, parenthesis, and submit buttons, and shows target, answer, and submissions left. No clear or backspace handling yet, and no Turn In button.
   - Keypad UI with button prefabs spawned per slot (not started). Buttons are currently placed by hand, and the Inspector arrays must match the hand sizes.
   - Test paper display showing submitted lines and target goals.
   - Flip/re-roll animation for spent buttons.
3. **Progression and economy.**
   - `GameManager` for GPA, round targets and submission counts, grade and bonus calculation, and the loss condition.
   - Desk Shop UI and purchase logic.
   - Test flow manager for question → shop → boss transitions.
4. **Boss modifiers and polish.**
   - Boss constraint checks on submission.
   - Hand-drawn fonts, pencil-scratch audio, paper textures, and screen shake on big ANS hits.

## Build / run / test

There is no CLI build setup. Open the project in the Unity Editor (via Unity Hub) to run, build, and test. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, and the generated `*.csproj`/`.slnx` files are Unity-generated, so don't hand-edit them.

The Unity Test Framework (`com.unity.test-framework`) is installed. Tests are NUnit, EditMode only. `Assets/Tests/CalcTest.cs` covers the evaluator (addition, subtraction, divide by zero, syntax error, `ANS`, parentheses). `Assets/Tests/MathPoolTest.cs` covers `MathPool` (single entry, zero weight, weight raising, a seeded frequency check, empty pool). `KeyHand`, `Round`, and `Token` equality have no tests yet. Run them from the Unity Test Runner window, or headlessly with `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -quit`. Add new tests for any new operator or evaluator behavior.

## Architecture

Code is split into two assembly definitions:
- `Assets/Scripts/Arithmophobia.asmdef` (runtime). It references TextMeshPro by GUID.
- `Assets/Tests/Tests.asmdef` (Editor-only, `UNITY_INCLUDE_TESTS`). It references `Arithmophobia`, so new runtime code under `Assets/Scripts/` is visible to tests.

### `Assets/Scripts/EquationEvaluator.cs`

A pure C# calculator core with no `MonoBehaviour` or Unity dependencies. It evaluates equations built from a `List<Token>` rather than by parsing strings:

- `Token` (struct, with `TokenType` enum): number, operator, `ANS` (the previous result), and left/right parentheses. Create tokens with the `Token.Create*` factories: `CreateNumber`, `CreateOperator`, `CreateAns`, `CreateLeftParen`, `CreateRightParen`.
- `EquationEvaluator.EvaluateTokens(tokens, currentAns)` converts infix to RPN with the shunting-yard algorithm (`ConvertToRPN`), then evaluates it (`CalculateRPN`). It returns an `EvaluationResult` struct with an `EvaluationStatus` (`Success`, `SyntaxError`, `DivideByZero`) and never throws to the caller.
- Operators are `+ - * / ^`. `^` is right-associative and has the highest precedence. Precedence and associativity live in `GetPrecedence` and `IsRightAssociative`, so add new operators there and in the `switch` in `CalculateRPN`.

`Token` has custom equality: two tokens are equal when their `Type` and `Symbol` match (`Equals`, `GetHashCode`, `==`, `!=`). `Value` is deliberately ignored, since comparing doubles is fragile.

### `Assets/Scripts/MathPool.cs`

Pure C#. A weighted bag of `Token`s, stored as a `List<WeightedEntry>` (`WeightedEntry` is a class, because mutating a struct inside a `List` would change a copy). `AddKey(token, num)` adds a token or raises its weight, and weights never go below zero. `DrawKey()` does a weighted pick from a running total and throws `InvalidOperationException` if the total weight is 0. The constructor takes an optional `System.Random` so tests can seed it. `CreateDefaultOperands` (digits 0 to 9) and `CreateDefaultOperators` build the starting pools. The default operator pool still includes `*` and `/`, which the design says are shop unlocks.

### `Assets/Scripts/KeyHand.cs`

Pure C#. `KeyHand` owns the operand and operator pools and a `List<KeySlot>` for each (6 operand slots and 2 operator slots by default). `KeySlot` is a class holding a `Token` and an `IsSpent` flag. Slots are addressed by index, never by token, because two slots can hold the same key. `Spend*` marks a slot spent, `Refresh()` redraws only spent slots, and `UnspendAll()` restores spent slots without redrawing (used on a failed equation). The hand has no softlock guarantee. A hand with no `+` or `-` is allowed.

### `Assets/Scripts/Round.cs`

Pure C#. `Round` holds the target, `SubmissionsLeft`, `CurrentAns` (the last successful answer), and a `RoundStatus` (`InProgress`, `Won`, `Lost`). It is still on the old rule (see Round rules above) and does not yet ignore submissions after the round ends.

### `Assets/Scripts/KeypadController.cs`

A `MonoBehaviour` that UI buttons call into. It creates a `KeyHand` and a `Round` in `Start()`, builds the current `List<Token>`, merges consecutive operand tokens into one number (this assumes plain digits), and shows the equation, target, answer, and submissions left in assigned `TMP_Text` fields. Operand and operator buttons come from public `Button[]` arrays filled in the Inspector, and a button's index is its slot. `RefreshAllStates()` pushes the hand and round state into the UI. On a successful submit it passes the result to the `Round` and refreshes the hand. On an error it restores the spent keys. It still has a separate `roundDone` bool that duplicates `Round.Status`, and the win/lose and GPA handling are placeholder comments.

### Known quirks (fix them if you touch the file)

- `CalculateRPN` has no `default` case for unknown operator symbols, so an unrecognized symbol silently drops the operands.
- Unary minus is not supported. `-3` fails with "Missing Operands".
- The Inspector button arrays in `KeypadController` must be sized to match `KeyHand`'s slot counts. A mismatch is logged in `Start()`, but the log runs after the first refresh, which throws first.
- `CalcTest.cs` still contains the template `UnityTest` stub (`CalcTestWithEnumeratorPasses`), which asserts nothing.

## Repo notes

- Commit `.meta` files together with their assets, because Unity GUIDs depend on them.
- `Assets/TextMesh Pro/` is the stock TMP import (examples and fonts). It is not game code.
- `Assets/_Recovery/` is an untracked Unity scene-recovery folder (autosave from a crash). Don't commit it.
- Unity config in `ProjectSettings/` and `Assets/Settings/` is checked in. Expect diffs there when the editor is opened, and avoid committing unintended editor-driven changes.
