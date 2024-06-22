namespace GNScript.Helpers;
public class BodyHandler : IDisposable
{
    private readonly Action _onDispose;

    public BodyHandler(Action onInit, Action onDispose)
    {
        if (onInit == null)
            throw new ArgumentNullException(nameof(onInit));

        if (onDispose == null)
            throw new ArgumentNullException(nameof(onDispose));

        onInit.Invoke();
        _onDispose = onDispose;
    }

    public void Dispose()
    {
        _onDispose();
    }
}
