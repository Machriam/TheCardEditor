using TheCardEditor.Shared;

namespace TheCardEditor.Services;

public interface IServiceAccessor<TService> where TService : class
{
    void Execute<TParameter>(Action<TService, TParameter> function, TParameter parameter) where TParameter : class;

    TReturn Execute<TReturn, TParameter>(Func<TService, TParameter, TReturn> function, TParameter parameter) where TParameter : class;

    TReturn Execute<TReturn>(Func<TService, TReturn> function);
}

public class ServiceAccessor<TService> : IServiceAccessor<TService> where TService : class
{
    private readonly TService _service;

    public ServiceAccessor(TService service)
    {
        _service = service;
    }

    public TReturn Execute<TReturn>(Func<TService, TReturn> function)
    {
        return function.Invoke(_service);
    }

    public void Execute<TParameter>(Action<TService, TParameter> function, TParameter parameter) where TParameter : class
    {
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        function.Invoke(_service, parameter);
    }

    public TReturn Execute<TReturn, TParameter>(Func<TService, TParameter, TReturn> function, TParameter parameter) where TParameter : class
    {
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        return function.Invoke(_service, parameter);
    }
}
