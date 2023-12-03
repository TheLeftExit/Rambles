using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class RazorComponentRenderer
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_passiveHtmlRenderer")]
    private extern static ref StaticHtmlRenderer GetStaticHtmlRenderer(this HtmlRenderer htmlRenderer);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Dispatcher>k__BackingField")]
    private extern static ref Dispatcher GetDispatcher(this StaticHtmlRenderer staticHtmlRenderer);

    public static HtmlRenderer HtmlRendererPatched { get; }

    static RazorComponentRenderer()
    {
        HtmlRendererPatched = new HtmlRenderer(new DummyServiceProvider(), NullLoggerFactory.Instance);
        HtmlRendererPatched.GetStaticHtmlRenderer().GetDispatcher() = new DummyDispatcher();
    }

    public static async Task<string> RenderAsync<T>(this T component, IDictionary<string, object?>? parameters = null) where T : ComponentBase
    {
        parameters ??= typeof(T).GetProperties()
            .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null)
            .ToDictionary(x => x.Name, x => x.GetValue(component));
        var parameterView = ParameterView.FromDictionary(parameters);
        var htmlRootElement = await HtmlRendererPatched.RenderComponentAsync<T>(parameterView);
        return htmlRootElement.ToHtmlString();
    }

    private class DummyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private class DummyDispatcher : Dispatcher
    {
        public override bool CheckAccess() => true;
        public override Task InvokeAsync(Action workItem) => new(workItem);
        public override Task InvokeAsync(Func<Task> workItem) => workItem();
        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem) => new(workItem);
        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem) => workItem();
    }
}