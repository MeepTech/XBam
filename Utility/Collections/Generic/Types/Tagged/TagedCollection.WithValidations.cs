using System;
using System.Collections.Generic;
namespace Meep.Tech.Collections.Generic {

  public partial class TagedCollection<TTag, TValue> {
    /// <summary>
    /// A tagged collection with optional validatiosn before adding or removing items.
    /// </summary>
    public class WithValidations : TagedCollection<TTag, TValue> {
      Func<IEnumerable<TTag>, TValue, bool> _validateAdd;
      Func<IEnumerable<TTag>, TValue, bool> _validateRemoveTagsForItem;
      Func<TValue, bool> _validateRemoveValue;

      /// <summary>
      /// Make a new tagged collection with validations
      /// </summary>
      public WithValidations(
        Func<IEnumerable<TTag>, TValue, bool> validateAdd = null,
        Func<IEnumerable<TTag>, TValue, bool> validateRemoveTagsForItem = null,
        Func<TValue, bool> validateRemoveValue = null
      ) : base() {
        _validateAdd = validateAdd;
        _validateRemoveTagsForItem = validateRemoveTagsForItem;
        _validateRemoveValue = validateRemoveValue;
      }

      ///<summary><inheritdoc/></summary>
      public override void Add(IEnumerable<TTag> tags, TValue value) {
        if(_validateAdd?.Invoke(tags, value) ?? true) {
          base.Add(tags, value);
        }
      }
      
      ///<summary><inheritdoc/></summary>
      public override bool Remove(TValue value) 
        => _validateRemoveValue?.Invoke(value) ?? true ? base.Remove(value) : false;
      
      ///<summary><inheritdoc/></summary>
      public override bool RemoveTagsForItem(IEnumerable<TTag> tags, TValue value) 
        => (_validateRemoveTagsForItem?.Invoke(tags, value) ?? true) && base.RemoveTagsForItem(tags, value);
      
      ///<summary><inheritdoc/></summary>
      public override bool RemoveValuesFor(TTag tag) {
        foreach(TValue value in this[tag]) {
          if(!(_validateRemoveValue?.Invoke(value) ?? true)) {
            return false;
          }
        }
        return base.RemoveValuesFor(tag);
      }
    }
  }
}