using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Text;

/// <summary>
/// Represents a consistent hash ring used for load balancing work amongst work agents.
/// </summary>
/// <remarks>
/// The implementation accepts arbitrary work agent and work items, as long as they have a string representation that is returned by <see cref="object.ToString"/>.
/// The string representation is used for hashing, so it is important that it is consistent across agents and work items.
/// </remarks>
public sealed class ConsistentHashRing<TAgent, TWork>
    where TAgent : class
    where TWork : class
{
    private readonly NonCryptographicHashAlgorithm hasher;

    private readonly Encoding encoding;

    private readonly SortedList<string, TAgent> agents;

    private readonly SortedList<string, TWork> work;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsistentHashRing{TAgent, TWork}"/> class.
    /// </summary>
    /// <param name="hasher">The hasher implementation to use.</param>
    /// <param name="encoding">The optional encoding to rely on. Defaults to <see cref="Encoding.ASCII"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when a hasher is not specified.</exception>
    public ConsistentHashRing(NonCryptographicHashAlgorithm hasher, Encoding encoding = null)
    {
        this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        this.encoding = encoding ?? Encoding.ASCII;

        this.agents = new SortedList<string, TAgent>();
        this.work = new SortedList<string, TWork>();
    }

    /// <summary>
    /// Adds a work agent amongst which work will be load balanced.
    /// </summary>
    /// <param name="agent">The work agent.</param>
    /// <param name="instanceCount">The number of virtual instances to create in order to improve load balancing.</param>
    /// <exception cref="InvalidDataException">Thrown when the specified agent does not have a string representation.</exception>
    public void AddWorkAgent(TAgent agent, ushort instanceCount = 10)
    {
        const int shortSize = sizeof(ushort);
        string agentId = agent.ToString() ?? throw new InvalidDataException();
        for (ushort i = 0; i < instanceCount; i++)
        {
            byte[] buffer = new byte[this.encoding.GetByteCount(agentId) + shortSize];
            Span<byte> bufferSpan = buffer;
            int bytes = encoding.GetBytes(agentId, buffer);
            BitConverter.TryWriteBytes(bufferSpan.Slice(bytes, shortSize), i);
            this.hasher.Append(buffer);
            byte[] hash = this.hasher.GetHashAndReset();
            string baseHash = Convert.ToHexString(hash);
            this.agents.Add(baseHash, agent);
        }
    }

    /// <summary>
    /// Adds work to be load balanced.
    /// </summary>
    /// <param name="work">The work.</param>
    /// <exception cref="InvalidDataException">Thrown when the specified work does not have a string representation.</exception>
    public void AddWork(TWork work)
    {
        string workId = work.ToString() ?? throw new InvalidDataException();
        byte[] buffer = encoding.GetBytes(workId);
        this.hasher.Append(buffer);
        byte[] hash = this.hasher.GetHashAndReset();
        string baseHash = Convert.ToHexString(hash);
        this.work.Add(baseHash, work);
    }

    /// <summary>
    /// Load balances the work amongst agents.
    /// </summary>
    /// <param name="workComparer">The optional work comparer.</param>
    /// <returns>A dictionary of work-to-agent mappings.</returns>
    public IReadOnlyDictionary<TWork, TAgent> LoadBalance(IEqualityComparer<TWork> workComparer = null)
    {
        Dictionary<TWork, TAgent> result = new(workComparer);

        IEnumerator<KeyValuePair<string, TAgent>> agents = this.agents.GetEnumerator();
        IEnumerator<KeyValuePair<string, TWork>> work = this.work.GetEnumerator();

        if (agents.MoveNext() && work.MoveNext())
        {
            bool force = false;
            while (true)
            {
                bool condition = force || StringComparer.Ordinal.Compare(agents.Current.Key, work.Current.Key) >= 0;

                // agent sorts after work
                if (condition)
                {
                    result.Add(work.Current.Value, agents.Current.Value);
                    if (!work.MoveNext())
                    {
                        // end of work
                        break;
                    }
                }
                else
                {
                    if (!agents.MoveNext())
                    {
                        // end of agents, restart agents to remap
                        agents.Dispose();
                        agents = this.agents.GetEnumerator();
                        agents.MoveNext();
                        force = true;
                    }
                }
            }
        }

        agents.Dispose();
        work.Dispose();

        return result;
    }

    /// <summary>
    /// Clears the agents and the work.
    /// </summary>
    public void Clear()
    {
        this.agents.Clear();
        this.work.Clear();
    }

    /// <summary>
    /// Clears the work.
    /// </summary>
    public void ClearWork()
    {
        this.work.Clear();
    }
}
