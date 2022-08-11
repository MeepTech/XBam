using Meep.Tech.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.XBam {
  public partial class Universe {

    /// <summary>
    /// An extra context that stores all other extra contexts for a universe.
    /// </summary>
    public class ExtraContextsData : ExtraContext, IEnumerable<ExtraContext> {
      IEnumerable<PropertyInfo> _overrideableFields;

      internal readonly Dictionary<Type, ExtraContext> _extraContexts
        = new();

      ///<summary><inheritdoc/></summary>
      public ExtraContextsData(Universe universe) {
        Universe = universe;
        _collectAllOverridableFields();
        _initializeAllOverrideableFieldsToNoOps();
      }

      ///<summary><inheritdoc/></summary>
      public IEnumerator<ExtraContext> GetEnumerator()
        => _extraContexts.Values.Distinct().GetEnumerator();

      ///<summary><inheritdoc/></summary>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

      internal void _addAllOverrideDelegates<TExtraContext>(TExtraContext extraContext) where TExtraContext : ExtraContext {
        foreach (var field in _overrideableFields) {
          Delegate providedLogicInOverride;
          // if a field is overrided in an extra context, we want to add it to the events we call when calling that via the ExtraContexts object.
          if ((providedLogicInOverride = (Delegate)field.GetValue(extraContext)) != null) {
            Delegate currentLogic = (Delegate)field.GetValue(this);
            if (currentLogic is null) {
              currentLogic = field.PropertyType.BuildNoOpDelegate();
            }
            Delegate combinedLogic = Delegate.Combine(currentLogic, providedLogicInOverride);
            field.SetValue(this, combinedLogic);
          }
        }
      }

      void _collectAllOverridableFields() {
        /// get all delegate fields that are overridable that ExtraContext defines.
        _overrideableFields = typeof(ExtraContext).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          .Where(p => p.GetMethod.GetBaseDefinition().DeclaringType == typeof(ExtraContext) && typeof(Delegate).IsAssignableFrom(p.PropertyType));
      }

      void _initializeAllOverrideableFieldsToNoOps() {
        foreach (var field in _overrideableFields) {
          Delegate currentLogic = (Delegate)field.GetValue(this);
          if (currentLogic is null) {
            field.SetValue(this, field.PropertyType.BuildNoOpDelegate());
          }
        }
      }
    }
  }
}
