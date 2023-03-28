using Microsoft.Extensions.DependencyInjection;
using TheCardEditor.DataModel.DataModel;
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
    private readonly IServiceProvider _serviceProvider;

    public ServiceAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private TService GetService =>
        (TService)Activator.CreateInstance(typeof(TService), _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>())!;

    public TReturn Execute<TReturn>(Func<TService, TReturn> function)
    {
        return function.Invoke(GetService);
    }

    public void Execute<TParameter>(Action<TService, TParameter> function, TParameter parameter) where TParameter : class
    {
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        function.Invoke(GetService, parameter);
    }

    public TReturn Execute<TReturn, TParameter>(Func<TService, TParameter, TReturn> function, TParameter parameter) where TParameter : class
    {
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        return function.Invoke(GetService, parameter);
    }
}
