using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class KeypadController : MonoBehaviour
{
    public TMP_Text equationDisplay; // Reference to the TextMeshProUGUI component for displaying the equation
    bool isNewEquation = true; // Flag to indicate if a new equation is being entered
    double currentAns = 0; // Store the last answer value
    List<Token> currentTokens = new List<Token>();
    public void OnDigitPressed(int digit)
    {
        // Handle the digit button press event here
        AppendToDisplay(digit.ToString()); // Clear the display for a new equation

        if (currentTokens.Count > 0 && currentTokens[currentTokens.Count - 1].Type == TokenType.Number)
        {
            // If the last token is a number, we need to combine it with the new digit
            double lastNumber = currentTokens[currentTokens.Count - 1].Value;
            double newNumber = lastNumber * 10 + digit; // Combine the digits
            currentTokens[currentTokens.Count - 1] = Token.CreateNumber(newNumber); // Update the last token
        }
        else currentTokens.Add(Token.CreateNumber(digit));
    }
    public void OnOperatorPressed(string op)
    {
        // Handle the operator button press event here
        AppendToDisplay(op);
        currentTokens.Add(Token.CreateOperator(op));
    }
    public void OnAnsPressed()
    {
        // Handle the ANS button press event here
        AppendToDisplay("ANS");
        currentTokens.Add(Token.CreateAns());
    }
    public void OnLParPressed()
    {
        AppendToDisplay("(");
        currentTokens.Add(Token.CreateLeftParen());
    }
    public void OnRParPressed()
    {
        AppendToDisplay(")");
        currentTokens.Add(Token.CreateRightParen());
    }  
    public void OnSubmitPressed()
    {
        // Handle the submit event here
        EvaluationResult result = EquationEvaluator.EvaluateTokens(currentTokens, currentAns);
        if (result.Status == EvaluationStatus.Success)
        {
            currentAns = result.Value; // Store the result for future ANS usage
            equationDisplay.text = "Result: " + result.Value;
        }
        else equationDisplay.text = "Error: " + result.ErrorMessage;
        currentTokens.Clear(); // Clear the current tokens after evaluation
        isNewEquation = true; // Reset the flag for a new equation
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