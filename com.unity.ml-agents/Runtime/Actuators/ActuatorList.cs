using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Unity.MLAgents.Actuators
{
    /// <summary>
    /// A list of IActuators
    /// </summary>
    public class ActuatorList : IList<IActuator>
    {
        float[] m_ContinuousActions;
        int[] m_DiscreteActions;
        IList<IActuator> m_Actuators;

        /// <summary>
        /// Create an ActuatorList with a preset capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the list to create.</param>
        public ActuatorList(int capacity = 0)
        {
            m_Actuators = new List<IActuator>(capacity);
        }

        /// <summary>
        /// Returns the previously stored actions for the actuators in this list.
        /// </summary>
        public float[] storedContinuousActions
        {
            get { return m_ContinuousActions; }
        }

        /// <summary>
        /// Returns the previously stored actions for the actuators in this list.
        /// </summary>
        public int[] storedDiscreteActions
        {
            get { return m_DiscreteActions; }
        }

        /// <summary>
        /// Ensures that the action buffer size is correct based on the number of
        /// actions each actuator has.
        /// </summary>
        public void EnsureActionBufferSize()
        {
            var continuousSize = 0;
            var discreteSize = 0;
            for (var i = 0; i < m_Actuators.Count; i++)
            {
                continuousSize += m_Actuators[i].ContinuousActuatorSpace.NumActions;
                discreteSize += m_Actuators[i].DiscreteActuatorSpace.NumActions;
            }

            m_ContinuousActions = new float[continuousSize];
            m_DiscreteActions = new int[discreteSize];
        }

        internal void UpdateActions(float[] fullActionBuffer)
        {
            // This method exists as a bridge between the old and the new.
            // The old is where we treated all actions as a float array.
            // The new is where we want to treat discrete and continuous actions
            // as separate buffers and handle them accordingly
            if (m_ContinuousActions.Length > 0)
            {
                UpdateActions(fullActionBuffer, Array.Empty<int>());
            }
            else if (m_DiscreteActions.Length > 0)
            {
                UpdateActions(Array.Empty<float>(),
                    Array.ConvertAll(fullActionBuffer,
                        x => (int)x));
            }
        }

        /// <summary>
        /// Updates the local action buffer with the action buffer passed in.  If the buffer
        /// passed in is null, the local action buffer will be cleared.
        /// </summary>
        /// <param name="continuousActionBuffer">The action buffer which contains all of the
        /// actions for the IActuators in this list.</param>
        public void UpdateActions(float[] continuousActionBuffer, int[] discreteActionBuffer)
        {
            UpdateActionArray(continuousActionBuffer, m_ContinuousActions);
            UpdateActionArray(discreteActionBuffer, m_DiscreteActions);
        }

        static void UpdateActionArray<T>(T[] sourceActionBuffer, T[] destination)
        {
            if (sourceActionBuffer == null)
            {
                Array.Clear(destination, 0, destination.Length);
            }
            else
            {
                Debug.Assert(sourceActionBuffer.Length == destination.Length,
                    "fullActionBuffer is a different size than m_Actions.");

                Array.Copy(sourceActionBuffer, destination, destination.Length);
            }
        }

        /// <summary>
        /// Iterates through all of the IActuators in this list and calls their
        /// <see cref="IActionReceiver.OnActionReceived"/> method on them.
        /// </summary>
        public void ExecuteActions()
        {
            var continuousStart = 0;
            var discreteStart = 0;
            for (var i = 0; i < m_Actuators.Count; i++)
            {
                var actuator = m_Actuators[i];
                var numContinuousActions = actuator.ContinuousActuatorSpace.NumActions;
                var numDiscreteActions = actuator.DiscreteActuatorSpace.NumActions;
                var continuousActions = new ActionSegment<float>(m_ContinuousActions, continuousStart, numContinuousActions);
                var discreteActions = new ActionSegment<int>(m_DiscreteActions, discreteStart, numDiscreteActions);

                actuator.OnActionReceived(continuousActions, discreteActions);
                continuousStart += numContinuousActions;
                discreteStart += numDiscreteActions;
            }
        }

        /// <summary>
        /// Sorts the <see cref="IActuator"/>s according to their <see cref="IActuator.GetName"/> value.
        /// </summary>
        public void SortActuators()
        {
            ((List<IActuator>) m_Actuators).Sort((x,
                y) => x.GetName()
                .CompareTo(y.GetName()));
        }

        /// <summary>
        /// Resets the data of the local action buffer to all 0f.
        /// </summary>
        public void ResetData()
        {
            Array.Clear(m_ContinuousActions, 0, m_ContinuousActions.Length);
            Array.Clear(m_DiscreteActions, 0, m_DiscreteActions.Length);
            for (var i = 0; i < m_Actuators.Count; i++)
            {
                m_Actuators[i].ResetData();
            }
        }

        /*********************************************************************************
         * IList implementation that delegates to m_Actuators List.                      *
         *********************************************************************************/

        /// <summary>
        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        public IEnumerator<IActuator> GetEnumerator()
        {
            return m_Actuators.GetEnumerator();
        }

        /// <summary>
        /// <inheritdoc cref="IList{T}.GetEnumerator"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_Actuators).GetEnumerator();
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Add"/>
        /// </summary>
        /// <param name="item"></param>
        public void Add(IActuator item)
        {
            m_Actuators.Add(item);
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Clear"/>
        /// </summary>
        public void Clear()
        {
            m_Actuators.Clear();
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Contains"/>
        /// </summary>
        public bool Contains(IActuator item)
        {
            return m_Actuators.Contains(item);
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.CopyTo"/>
        /// </summary>
        public void CopyTo(IActuator[] array, int arrayIndex)
        {
            m_Actuators.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Remove"/>
        /// </summary>
        public bool Remove(IActuator item)
        {
            return m_Actuators.Remove(item);
        }

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.Count"/>
        /// </summary>
        public int Count => m_Actuators.Count;

        /// <summary>
        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        /// </summary>
        public bool IsReadOnly => m_Actuators.IsReadOnly;

        /// <summary>
        /// <inheritdoc cref="IList{T}.IndexOf"/>
        /// </summary>
        public int IndexOf(IActuator item)
        {
            return m_Actuators.IndexOf(item);
        }

        /// <summary>
        /// <inheritdoc cref="IList{T}.Insert"/>
        /// </summary>
        public void Insert(int index, IActuator item)
        {
            m_Actuators.Insert(index, item);
        }

        /// <summary>
        /// <inheritdoc cref="IList{T}.RemoveAt"/>
        /// </summary>
        public void RemoveAt(int index)
        {
            m_Actuators.RemoveAt(index);
        }

        /// <summary>
        /// <inheritdoc cref="IList{T}.this"/>
        /// </summary>
        public IActuator this[int index]
        {
            get => m_Actuators[index];
            set => m_Actuators[index] = value;
        }

    }
}
