using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Base
{
    /// <summary>
    /// エディットコンポーネントベース
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EditBase<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : CustomComponentBase<T>
    {

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [Parameter]
        public string ParsingErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// 無効フラグ
        /// </summary>
        [Parameter]
        public bool IsDisabled { get; set; } = false;
        /// <summary>
        /// プレースホルダー
        /// </summary>
        [Parameter]
        public string PlaceHolder { get; set; } = string.Empty;
        /// <summary>
        /// 変換抽象化メソッド
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected abstract bool ConvetTo(string? value, out T? result);
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="validationErrorMessage"></param>
        /// <returns></returns>
        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out T result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
        }
        /// <summary>
        /// 文字列変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string? FormatValueAsString(T? value)
        {
            var ressult = value?.ToString() ?? null;
            return ressult;
        }
    }
}
