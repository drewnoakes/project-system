// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Threading.Tasks.Dataflow;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.Properties
{
    /// <summary>
    ///     Abstract class for providers that process values from evaluation.
    /// </summary>
    internal abstract class SupportedValuesProvider : ChainedProjectValueDataSourceBase<ICollection<IEnumValue>>, IDynamicEnumValuesProvider, IDynamicEnumValuesGenerator
    {
        protected IProjectSubscriptionService SubscriptionService { get; }

        protected abstract string[] RuleNames { get; }

        protected SupportedValuesProvider(
            ConfiguredProject project,
            IProjectSubscriptionService subscriptionService)
            : base(project, synchronousDisposal: false, registerDataSource: false)
        {
            SubscriptionService = subscriptionService;
        }

        protected override IDisposable? LinkExternalInput(ITargetBlock<IProjectVersionedValue<ICollection<IEnumValue>>> targetBlock)
        {
            IProjectValueDataSource<IProjectSubscriptionUpdate> source = SubscriptionService.ProjectRuleSource;

            // Transform the values from evaluation to structure from the rule schema.
            DisposableValue<ISourceBlock<IProjectVersionedValue<ICollection<IEnumValue>>>> transformBlock = source.SourceBlock.TransformWithNoDelta(
                update => update.Derive(Transform),
                suppressVersionOnlyUpdates: false,
                ruleNames: RuleNames);

            // Set the link up so that we publish changes to target block.
            transformBlock.Value.LinkTo(targetBlock, DataflowOption.PropagateCompletion);

            // Join the source blocks, so if they need to switch to UI thread to complete 
            // and someone is blocked on us on the same thread, the call proceeds.
            JoinUpstreamDataSources(source);

            return transformBlock;
        }

        protected abstract ICollection<IEnumValue> Transform(IProjectSubscriptionUpdate input);

        protected abstract IEnumValue ToEnumValue(KeyValuePair<string, IImmutableDictionary<string, string>> item);

        bool IDynamicEnumValuesGenerator.AllowCustomValues => false;

        Task<IEnumValue?> IDynamicEnumValuesGenerator.TryCreateEnumValueAsync(string userSuppliedValue) => TaskResult.Null<IEnumValue>();

        public Task<IDynamicEnumValuesGenerator> GetProviderAsync(IList<NameValuePair>? options)
        {
            return Task.FromResult<IDynamicEnumValuesGenerator>(this);
        }

        public async Task<ICollection<IEnumValue>> GetListedValuesAsync()
        {
            using (JoinableCollection.Join())
            {
                IProjectVersionedValue<ICollection<IEnumValue>> snapshot = await SourceBlock.ReceiveAsync();

                return snapshot.Value;
            }
        }
    }
}
