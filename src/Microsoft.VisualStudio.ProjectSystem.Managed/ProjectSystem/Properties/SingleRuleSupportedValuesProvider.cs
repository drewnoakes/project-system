// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.Build.Framework.XamlTypes;

namespace Microsoft.VisualStudio.ProjectSystem.Properties
{
    internal abstract class SingleRuleSupportedValuesProvider : SupportedValuesProvider
    {
        /// <summary>
        /// Specifies if a 'None' value should be added to the resulting list.
        /// </summary>
        private readonly bool _useNoneValue;

        private readonly string _ruleName;

        protected sealed override string[] RuleNames => new string[] { _ruleName };

        protected SingleRuleSupportedValuesProvider(ConfiguredProject project, IProjectSubscriptionService subscriptionService, string ruleName, bool useNoneValue = false) : base(project, subscriptionService)
        {
            _ruleName = ruleName;
            _useNoneValue = useNoneValue;
        }

        protected override ICollection<IEnumValue> Transform(IProjectSubscriptionUpdate input)
        {
            var snapshot = input.CurrentState[_ruleName] as IDataWithOriginalSource<KeyValuePair<string, IImmutableDictionary<string, string>>>;

            Assumes.NotNull(snapshot);

            int capacity = snapshot.SourceData.Count + (_useNoneValue ? 1 : 0);
            var list = new List<IEnumValue>(capacity);

            if (_useNoneValue)
            {
                list.Add(new PageEnumValue(new EnumValue
                {
                    Name = string.Empty,
                    DisplayName = Resources.Property_NoneValue
                }));
            }

            list.AddRange(snapshot.SourceData.Select(ToEnumValue));

            return list;
        }
    }
}
