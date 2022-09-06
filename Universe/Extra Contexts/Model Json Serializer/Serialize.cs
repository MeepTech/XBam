using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Meep.Tech.XBam.Json {

  /// <summary>
  /// Access default json serialize logic
  /// </summary>
  public static class Serialize {

    /// <summary>
    /// Make a model into a jobject
    /// </summary>
    public static JObject ToJObject(IModel model, JsonSerializer? serializerOverride = null)
      => model.ToJson(serializerOverride);

    /// <summary>
    /// Make a model into a json string
    /// </summary>
    public static string ToString(IModel model, JsonSerializer? serializerOverride = null)
      => model.ToJson(serializerOverride).ToString();
  }
  
  /// <summary>
   /// Access default json serialize logic
   /// </summary>
  public static class Serialize<TModel> where TModel : IModel {

    /// <summary>
    /// Make a model into a jobject
    /// </summary>
    public static JObject ToJObject(TModel model, JsonSerializer? serializerOverride = null)
      => model.ToJson(serializerOverride);

    /// <summary>
    /// Make a model into a json string
    /// </summary>
    public static string ToString(TModel model, JsonSerializer? serializerOverride = null)
      => model.ToJson(serializerOverride).ToString();
  }
}