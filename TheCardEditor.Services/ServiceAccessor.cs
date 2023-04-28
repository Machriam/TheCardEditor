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

    private static TService GetService(DataContext context) => (TService)Activator.CreateInstance(typeof(TService), context)!;

    public IList<TReturn> Execute<TReturn>(Func<TService, IEnumerable<TReturn>> function)
    {
        using var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        return function.Invoke(GetService(context)).ToList();
    }

    public TReturn Execute<TReturn>(Func<TService, TReturn> function)
    {
        using var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        return function.Invoke(GetService(context));
    }

    public void Execute<TParameter>(Action<TService, TParameter> function, TParameter parameter) where TParameter : class
    {
        using var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        function.Invoke(GetService(context), parameter);
    }

    public IList<TReturn> Execute<TReturn, TParameter>(Func<TService, TParameter, IEnumerable<TReturn>> function, TParameter parameter) where TParameter : class
    {
        using var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        return function.Invoke(GetService(context), parameter).ToList();
    }

    public TReturn Execute<TReturn, TParameter>(Func<TService, TParameter, TReturn> function, TParameter parameter) where TParameter : class
    {
        using var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        var isValid = parameter.ModelIsValid(out var errors);
        if (!isValid) throw new Exception(errors);
        return function.Invoke(GetService(context), parameter);
    }
}
