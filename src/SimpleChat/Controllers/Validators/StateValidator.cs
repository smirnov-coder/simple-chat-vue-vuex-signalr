using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// ������������� ������ �������� ����������� ��� ������������� ������ ���������, ������������ �������
    /// OAuth2-����������� ��� �������� ������, ���������� � ��������� <see cref="IContext"/>.
    /// </summary>
    public class StateValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "state";

        public StateValidator() : this(null)
        {
        }

        public StateValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
