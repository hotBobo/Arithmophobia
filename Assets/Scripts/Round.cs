public enum RoundStatus
{
    InProgress,
    Won,
    Lost
}
public class Round
{
    public Round(double target, int submissions)
    {
        TargetScore = target;
        SubmissionsLeft = submissions;
    }
    public RoundStatus UpdateSubmit(double submit)
    {
        if (Status != RoundStatus.InProgress)
            return Status;
        CurrentAns = submit;
        --SubmissionsLeft;
        if (CurrentAns >= TargetScore)
            return Status = RoundStatus.Won;
        else if (SubmissionsLeft <= 0)
            return Status = RoundStatus.Lost;
        return Status;
    }
    public double TargetScore {get; private set;}
    public double CurrentAns {get; private set;} = 0;
    public int SubmissionsLeft {get; private set;}
    public RoundStatus Status {get; private set;} = RoundStatus.InProgress;
}
