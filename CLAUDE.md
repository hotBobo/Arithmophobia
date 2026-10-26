# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## How to work with the user

The user is a student learning game design and C++, and is new to C# and Unity. This project is how they are learning, so the goal is their understanding, not just working code.

- Do not write or edit game code (anything under `Assets/Scripts/`, `Assets/Tests/`, scenes, or prefabs) unless the user explicitly asks you to.
- Instead, guide them. Explain how the existing code works, point to the relevant files and lines, introduce the C# or Unity concept involved (comparing it to C++ where that helps), and suggest the next step so they can write it themselves.
- When reviewing their code, explain what is wrong and why, and let them make the fix.
- Editing documentation such as this file is fine when asked.

## Project

Arithmophobia is a Unity game (Unity 6, editor version `6000.5.9f1`) using URP 17.6 and the new Input System (`com.unity.inputsystem`). The project is early-stage: there is one scene (`Assets/Scenes/SampleScene.unity`), a working equation evaluator with EditMode tests, and a bare-bones prototype keypad controller wired to TextMeshPro. Roadmap Phase 1 (math engine) is partly done, and Phase 2 (UI) has been started as a prototype.

## Game design (planned, mostly not yet implemented)

A student who struggles with math makes a deal to pass their exams. The presentation is a handwritten test booklet, with equations written on notebook paper. Upgrades are bought from an "under the desk" pencil-case shop.

**Core loop**
- Each question starts with a keypad grid of operands (digits, constants such as π and e, special tiles such as negative numbers) and operators.
- The player builds an equation from the available keys and submits it. It is evaluated with standard PEMDAS, and the result adds to the stage score.
- `ANS` carries the previous answer into the next equation, so answers can scale exponentially.
- Keys that were used morph into new keys drawn at random from the player's pool. Unused keys stay on the keypad.
- The keypad must always show at least one basic operator (`+` or `-`) so the player can't get softlocked.

**Pools**
- The operand pool is a custom bag of digits, constants, and special tiles.
- The operator pool has basic operators (`+`, `-`) with high drop rates. Exotic operators (`×`, `÷`, `x^y`, `!`) are unlocked in the shop.
- Draws are weighted random selections.

**GPA economy**
- GPA is capped at 4.00. Falling below 2.00 means Academic Probation, which is game over.
- Completing a test page earns +0.10 (C grade) up to +0.50 (A+, for doubling the target score).
- Shop purchases cost GPA (for example −0.40). Purchases include permanent keypad hardware upgrades, desk items, and deck-thinning ("White-Out").

**Test structure**
- Page 1 (Short Answer), then the Desk Shop, then Page 2 (Word Problem), then the Desk Shop, then Page 3 (Long Answer Boss).
- Boss quizzes apply modifiers such as "No Even Answers Allowed", "Fractions Inverted", and "Exact Target Match Only".

**Roadmap**
1. **Math engine (pure C#, no UI).**
   - Equation evaluator: done (no unary minus yet), see below.
   - ANS state tracking: prototype only. `KeypadController` holds `currentAns`; there is no standalone engine-level state.
   - `MathPool` with weighted random selection: not started.
2. **UI and input.**
   - Prototype keypad: `KeypadController` exposes `OnDigitPressed`, `OnOperatorPressed`, `OnAnsPressed`, `OnSubmitPressed` for UI buttons and writes to a `TMP_Text`. No parenthesis, clear, or backspace handling yet.
   - Keypad UI with button prefabs spawned per unlocked slot (not started).
   - Test paper display showing submitted lines and target goals.
   - Flip/re-roll animation for spent buttons.
3. **Progression and economy.**
   - `GameManager` for GPA, target scores, and the loss condition.
   - Desk Shop UI and purchase logic.
   - Test flow manager for question → shop → boss transitions.
4. **Boss modifiers and polish.**
   - Boss constraint checks on submission.
   - Hand-drawn fonts, pencil-scratch audio, paper textures, and screen shake on big ANS hits.

## Build / run / test

There is no CLI build setup. Open the project in the Unity Editor (via Unity Hub) to run, build, and test. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, and the generated `*.csproj`/`.slnx` files are Unity-generated, so don't hand-edit them.

The Unity Test Framework (`com.unity.test-framework`) is installed. Tests live in `Assets/Tests/CalcTest.cs` (NUnit, EditMode only) and cover addition, subtraction, divide by zero, syntax error, `ANS`, and parentheses. Run them from the Unity Test Runner window, or headlessly with `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -quit`. Add new tests for any new operator or evaluator behavior.

## Architecture

Code is split into two assembly definitions:
- `Assets/Scripts/Arithmophobia.asmdef` (runtime). It references TextMeshPro by GUID.
- `Assets/Tests/Tests.asmdef` (Editor-only, `UNITY_INCLUDE_TESTS`). It references `Arithmophobia`, so new runtime code under `Assets/Scripts/` is visible to tests.

### `Assets/Scripts/EquationEvaluator.cs`

A pure C# calculator core with no `MonoBehaviour` or Unity dependencies. It evaluates equations built from a `List<Token>` rather than by parsing strings:

- `Token` (struct, with `TokenType` enum): number, operator, `ANS` (the previous result), and left/right parentheses. Create tokens with the `Token.Create*` factories: `CreateNumber`, `CreateOperator`, `CreateAns`, `CreateLeftParen`, `CreateRightParen`.
- `EquationEvaluator.EvaluateTokens(tokens, currentAns)` converts infix to RPN with the shunting-yard algorithm (`ConvertToRPN`), then evaluates it (`CalculateRPN`). It returns an `EvaluationResult` struct with an `EvaluationStatus` (`Success`, `SyntaxError`, `DivideByZero`) and never throws to the caller.
- Operators are `+ - * / ^`. `^` is right-associative and has the highest precedence. Precedence and associativity live in `GetPrecedence` and `IsRightAssociative`, so add new operators there and in the `switch` in `CalculateRPN`.

### `Assets/Scripts/KeypadController.cs`

A `MonoBehaviour` prototype that UI buttons call into. It builds the current `List<Token>`, merges consecutive digits into one number token, tracks `currentAns`, and shows the equation or result in the assigned `TMP_Text equationDisplay`. On submit it evaluates, stores the result as the new `ANS` on success, shows the error message otherwise, and clears the tokens. It is hardcoded for digits, operator strings, and `ANS`. It will need rework for the planned operand and operator pools, random key morphing, and GPA and score tracking.

### Known quirks (fix them if you touch the file)

- `CalculateRPN` has no `default` case for unknown operator symbols, so an unrecognized symbol silently drops the operands.
- Unary minus is not supported. `-3` fails with "Missing Operands".
- `KeypadController` has no way to enter parentheses, even though the evaluator supports them.
- `CalcTest.cs` still contains the template `UnityTest` stub (`CalcTestWithEnumeratorPasses`), which asserts nothing.

## Repo notes

- Commit `.meta` files together with their assets, because Unity GUIDs depend on them.
- `Assets/TextMesh Pro/` is the stock TMP import (examples and fonts). It is not game code.
- `Assets/_Recovery/` is an untracked Unity scene-recovery folder (autosave from a crash). Don't commit it.
- Unity config in `ProjectSettings/` and `Assets/Settings/` is checked in. Expect diffs there when the editor is opened, and avoid committing unintended editor-driven changes.
