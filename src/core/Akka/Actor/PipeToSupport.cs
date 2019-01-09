//-----------------------------------------------------------------------
// <copyright file="PipeToSupport.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Akka.Actor
{
    /// <summary>
    /// Creates the PipeTo pattern for automatically sending the results of completed tasks
    /// into the inbox of a designated Actor
    /// </summary>
    public static class PipeToSupport
    {
        /// <summary>
        /// Pipes the output of a Task directly to the <paramref name="recipient"/>'s mailbox once
        /// the task completes
        /// </summary>
        /// <typeparam name="T">TBD</typeparam>
        /// <param name="taskToPipe">TBD</param>
        /// <param name="recipient">TBD</param>
        /// <param name="sender">TBD</param>
        /// <param name="success">
        ///     A function to map from the task result to an arbitrary message to send to the recipient when/if the result completes and is not canceled or faulted.
        ///     If null (default) the task result itself is sent
        /// </param>
        /// <param name="failure">
        ///     A function to map from the task exception to an arbitrary message to send to the recipient when/if the result is canceled or faulted.
        ///     If null (default) the exception is sent wrapped in a <see cref="Status.Failure">Status.Failure</see></param>
        /// <returns>TBD</returns>
        public static Task PipeTo<T>(this Task<T> taskToPipe, ICanTell recipient, IActorRef sender = null, Func<T, object> success = null, Func<Exception, object> failure = null)
        {
            sender = sender ?? ActorRefs.NoSender;
            return taskToPipe.ContinueWith(tresult =>
            {
                if (tresult.IsCanceled || tresult.IsFaulted)
                    recipient.Tell(failure != null
                        ? failure(tresult.Exception)
                        : new Status.Failure(tresult.Exception), sender);
                else if (tresult.IsCompleted)
                    recipient.Tell(success != null
                        ? success(tresult.Result)
                        : tresult.Result, sender);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Pipes the output of a Task directly to the <paramref name="recipient"/>'s mailbox once
        /// the task completes.  As this task has no result, only exceptions will be piped to the <paramref name="recipient"/>
        /// </summary>
        /// <param name="taskToPipe">TBD</param>
        /// <param name="recipient">TBD</param>
        /// <param name="sender">TBD</param>
        /// <param name="success">
        ///     A function that returns an arbitrary message to send to the recipient when/if the result completes and is not canceled or faulted.
        ///     If null (default) the task result itself is sent
        /// </param>
        /// <param name="failure">
        ///     A function to map from the task exception to an arbitrary message to send to the recipient when/if the result is canceled or faulted.
        ///     If null (default) the exception is sent wrapped in a <see cref="Status.Failure">Status.Failure</see></param>
        /// <returns>TBD</returns>
        /// <returns>TBD</returns>
        public static Task PipeTo(this Task taskToPipe, ICanTell recipient, IActorRef sender = null, Func<object> success = null, Func<Exception, object> failure = null)
        {
            sender = sender ?? ActorRefs.NoSender;
            return taskToPipe.ContinueWith(tresult =>
            {
                if (tresult.IsCanceled || tresult.IsFaulted)
                    recipient.Tell(failure != null
                        ? failure(tresult.Exception)
                        : new Status.Failure(tresult.Exception), sender);
                else if (tresult.IsCompleted && success != null)
                    recipient.Tell(success(), sender);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
}

