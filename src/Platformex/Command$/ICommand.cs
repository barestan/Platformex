namespace Platformex
{
    public interface ICommand { }

    public interface ICommand<T> : ICommand where T : Identity<T> { }
    
}