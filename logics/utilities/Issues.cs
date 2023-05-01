using System;

/*
 * Because of what is said at https://learn.microsoft.com/en-us/dotnet/api/system.exception?view=net-7.0#performance-considerations, I need to use a custom
 * system to handle input errors without throwing an exception. These can be used when in the Structure Editor, the user places a room at an invalid location ;
 * I need to notify him why his action is invalid, and for this I need to extract information from certain methods.
 *
 * While exceptions whould have been a good solution for this problem, the fact that it uses "a significant amount of system resources and execution time", 
 * means that a simple object returned by methods where null means there is no errors whould be more appropriate.
 *
 * Because input errors from the user are not critical and must be handled, this system will be called "Issue" and not "Errors", "Exceptions", and the like...
 */

/// <summary>
/// A user input error that must be handled.
/// </summary>
public abstract class Issue
{
    protected bool handled;

    ~Issue()
    {
        UnhandledException(this);
    }

    public void Handle()
    {
        if(handled)
            return;

        HandleExec();
        handled = true; 
    }

    protected virtual void HandleExec() { }

    public static void Handle(Issue issue) 
    {
        if(NeedHandling(issue))
            issue.Handle();
    }
    public static bool NeedHandling(Issue issue) => issue != null && !issue.handled;
    public static void UnhandledException(Issue issue)
    {
        if(NeedHandling(issue))
            throw new IssueUnhandledException(issue);
    }
}

/// <summary>
/// An issue with a message string.
/// </summary>
public abstract class MessageIssue : Issue
{
    protected string message;

    public MessageIssue(string message)
    {
        this.message = message;
    }

    public virtual string Message => message;
}

public class IssueUnhandledException : Exception
{
    public IssueUnhandledException(Issue issue) : base("Issue '" + issue.GetType() + "' wasn't handled.")
    { }
}