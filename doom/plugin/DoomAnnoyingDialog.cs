using Ivy;

namespace Ivy.Tendril.Plugin.Doom;

internal class DoomAnnoyingDialog(IState<bool> dialogOpen) : ViewBase
{
    private static readonly string[] Messages =
    [
        "Would you like to take a short survey about your experience with DOOM?",
        "Your free trial of DOOM has expired. Please enter your credit card to continue playing.",
        "Congratulations! You are the 1,000,000th player! Claim your prize now!",
        "We've updated our Privacy Policy. By continuing to play, you agree to share your every move with our trusted partners.",
        "Did you know? DOOM runs best on Ivy Framework. Rate us 5 stars in the App Store to dismiss this message.",
        "Your session will expire in 30 seconds. Please log in again to continue.",
        "A system update is required. DOOM will restart in 10 minutes.",
    ];

    public override object Build()
    {
        var message = Messages[Random.Shared.Next(Messages.Length)];

        return new Dialog(
            _ => dialogOpen.Set(false),
            new DialogHeader("IMPORTANT NOTICE"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                | Text.Block(message)
                | Text.Muted("This message was brought to you by the Tendril Plugin System.")
            ),
            new DialogFooter(
                new Button("Accept All Cookies", onClick: _ =>
                {
                    dialogOpen.Set(false);
                    return ValueTask.CompletedTask;
                }),
                new Button("Remind Me Later", onClick: _ =>
                {
                    dialogOpen.Set(false);
                    return ValueTask.CompletedTask;
                }, variant: ButtonVariant.Outline)
            )
        ).Width(Size.Rem(32));
    }
}
