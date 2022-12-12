namespace BeeEngine.Mediator
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
    }
}