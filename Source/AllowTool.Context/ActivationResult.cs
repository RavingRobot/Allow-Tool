using RimWorld;
using Verse;

namespace AllowTool.Context;

public class ActivationResult
{
	public const string SuccessIdSuffix = "_succ";

	public const string FailureIdSuffix = "_fail";

	public string Message { get; }

	public MessageTypeDef MessageType { get; }

	public static ActivationResult Success(string messageKey, params NamedArgument[] translateArgs)
	{
		return SuccessMessage((messageKey + "_succ").Translate(translateArgs));
	}

	public static ActivationResult SuccessMessage(string message)
	{
		return new ActivationResult(message, MessageTypeDefOf.TaskCompletion);
	}

	public static ActivationResult Failure(string messageKey, params NamedArgument[] translateArgs)
	{
		return FailureMessage((messageKey + "_fail").Translate(translateArgs));
	}

	public static ActivationResult FailureMessage(string message)
	{
		return new ActivationResult(message, MessageTypeDefOf.RejectInput);
	}

	public static ActivationResult FromCount(int designationCount, string baseMessageKey)
	{
		return (designationCount > 0) ? Success(baseMessageKey, designationCount) : Failure(baseMessageKey);
	}

	public ActivationResult()
	{
	}

	public ActivationResult(string message, MessageTypeDef messageType)
	{
		Message = message;
		MessageType = messageType;
	}

	public void ShowMessage()
	{
		if (Message != null && MessageType != null)
		{
			Messages.Message(Message, MessageType);
		}
	}
}
