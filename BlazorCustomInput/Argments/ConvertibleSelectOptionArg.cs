using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Argments
{
    /// <summary>
    /// コンバートタイプ選択ボックス
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TVale"></typeparam>
    public sealed class ConvertibleSelectOptionArg<TSource,TVale>
    {

        public TSource? SourceValue { get; }
        public TVale? Value { get; }

        public ConvertibleSelectOptionArg(TSource? sourceValue, TVale? value)
        {
            SourceValue = sourceValue;
            Value = value;
        }
    }
}
