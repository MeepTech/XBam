using System;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Used to test models and archetypes that have Generic arguments.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public class GenericTestArgumentAttribute : Attribute {
    public Type GenericArgumentType { get; }
    public int Order { get; }

    public GenericTestArgumentAttribute(System.Type GenericArgumentType, int Order) {
      this.GenericArgumentType = GenericArgumentType;
      this.Order = Order;
    }
  }
}
