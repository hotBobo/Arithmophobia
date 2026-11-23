using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class KeypadController : MonoBehaviour
{
    public TMP_Text equationDisplay, targetScoreDisplay, currentAnsDisplay, subsLeftDisplay;
    public Button[] operatorButtons, operandButtons;
    KeyHand hand;
    Round round;
    bool isNewEquation = true; // Flag to indicate if a new equation is being entered
    List<Token> currentTokens = new List<Token>();

    void Start()
    {
        hand = new KeyHand();
        round = new Round(10, 3);
        if (operatorButtons.Length != hand.operatorHandSize || operandButtons.Length != hand.operandHandSize)
            Debug.LogError("Button arrays do not match hand sizes.");
        RefreshAllStates();
    }
    public void OnOperandPressed(int slot)
    {
        if (hand.IsOperandSpent(slot) || round.Status != RoundStatus.InProgress) return;
        // Handle the digit button press event here
        if (currentTokens.Count > 0 && currentTokens[currentTokens.Count - 1].Type == TokenType.Number)
        {
            // If the last token is a number, we need to combine it with the new digit
            double temp = currentTokens[currentTokens.Count - 1].Value;
            temp = temp * 10 + hand.GetOperand(slot).Value; // Combine the digits
            currentTokens[currentTokens.Count - 1] = Token.CreateNumber(temp); // Update the last token
        }
        else currentTokens.Add(hand.GetOperand(slot)); // Add the new digit as a token
        hand.SpendOperand(slot);
        AppendToDisplay(hand.GetOperand(slot).Symbol);
        RefreshAllStates();
    }
    public void OnOperatorPressed(int slot)
    {
        if (hand.IsOperatorSpent(slot) || round.Status != RoundStatus.InProgress) return;
        // Handle the operator button press event here
        hand.SpendOperator(slot);
        AppendToDisplay(hand.GetOperator(slot).Symbol);
        currentTokens.Add(hand.GetOperator(slot));
        RefreshAllStates();
    }
    public void OnAnsPressed()
    {
        if (round.Status != RoundStatus.InProgress) return;
        // Handle the ANS button press event here
        AppendToDisplay("ANS");
        currentTokens.Add(Token.CreateAns());
    }
    public void OnLParPressed()
    {
        if (round.Status != RoundStatus.InProgress) return;
        AppendToDisplay("(");
        currentTokens.Add(Token.CreateLeftParen());
    }
    public void OnRParPressed()
    {
        if (round.Status != RoundStatus.InProgress) return;
        AppendToDisplay(")");
        currentTokens.Add(Token.CreateRightParen());
    }  
    public void OnSubmitPressed()
    {
        if (round.Status != RoundStatus.InProgress) return;
        // Handle the submit event here
        EvaluationResult result = EquationEvaluator.EvaluateTokens(currentTokens, round.CurrentAns);
        if (result.Status == EvaluationStatus.Success)
        {
            RoundStatus status = round.UpdateSubmit(result.Value);
            if (status != RoundStatus.InProgress)
            {
                if (status == RoundStatus.Lost)
                {
                    // Lost GPA
                }
                // Go to shop
            }
            equationDisplay.text = "Result: " + result.Value;
        }
        else 
        {
            equationDisplay.text = "Error: " + result.ErrorMessage;
            hand.UnspendAll();
        }
        currentTokens.Clear(); // Clear the current tokens after evaluation
        isNewEquation = true; // Reset the flag for a new equation
        hand.Refresh();
        RefreshAllStates();

    }
    void RefreshAllStates()
    {
        for(int i = 0; i < operandButtons.Length; ++i)
        {
            operandButtons[i].GetComponentInChildren<TMP_Text>().text = hand.GetOperand(i).Symbol;
            operandButtons[i].interactable = !hand.IsOperandSpent(i);
        }
        for(int i = 0; i < operatorButtons.Length; ++i)
        {
            operatorButtons[i].GetComponentInChildren<TMP_Text>().text = hand.GetOperator(i).Symbol;
            operatorButtons[i].interactable = !hand.IsOperatorSpent(i);
        }
        targetScoreDisplay.text = "Target Score: " + round.TargetScore;
        currentAnsDisplay.text = "CurrentAns: " + round.CurrentAns;
        subsLeftDisplay.text = "Submissions Left: " + round.SubmissionsLeft;
    }
    void AppendToDisplay(string text)
    {
        if (isNewEquation)
        {
            equationDisplay.text = text; // Clear the display for a new equation
            isNewEquation = false;
        }
        else equationDisplay.text += text;
    }
}

