namespace SniperPlugin.Communication;

public enum DecisionAction
{
    Ignore,
    Teleport
}

public record ItemDecision(DecisionAction Action, string Reason);
