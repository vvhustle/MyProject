using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.UI;

namespace YMatchThree.Core {
    public class MatchingTask : LevelGameplay.InternalTask {
        
        public override IEnumerator Logic() {
            var matched = false;

            using (var pool = MatchingPool.Get(context).Open()) { 
                yield return gameplay.Matching();
                yield return pool.Wait();
                
                if (pool.IsDirty())
                    Reactions.Get(field).Fill(Reaction.Type.Match);
                            
                if (pool.IsDirty())
                    gameplay.NextTask<GravityTask>();
                else
                    gameplay.NextTask<ReactionTask>();
            }
        }
    }

    public class MatchingPool : IDisposable {
        
        bool open = false;
        bool dirty = false;
        
        List<Ticket> tickets = new List<Ticket>();
        List<Ticket> closedtickets = new List<Ticket>();

        MatchingPool() {}
        
        public static MatchingPool Get(LiveContext context) {
            var result = context.GetArgument<MatchingPool>();
            
            if (result == null) {
                result = new MatchingPool();
                context.SetArgument(result);
            }
            
            return result;
        }

        public MatchingPool Open() {
            if (open)
                throw new Exception("The pool is already open");
            
            open = true;
            dirty = false;
            tickets.Clear();
            
            return this;
        }

        public void Dispose() {
            if (!open)
                throw new Exception("The pool is not open");
            
            open = false;
            dirty = false;
            tickets.Clear();
        }
        
        Ticket EmitTicket() {
            var ticket = closedtickets.Grab() ?? new Ticket();
            ticket.pool = this;
            return ticket;
        }
        
        public IDisposable Use() {
            if (!open)
                throw new Exception("The pool is not open. You can use the pool only during the MatchingTask");
            
            var ticket = EmitTicket();
            dirty = true;
            tickets.Add(ticket);
            return ticket;
        }
        
        void CloseTicket(Ticket ticket) {
            if (!tickets.Contains(ticket)) return;
            
            tickets.Remove(ticket);
            closedtickets.Add(ticket);
        }
        
        public bool IsEmpty() {
            return tickets.IsEmpty();
        }
        
        public bool IsDirty() {
            return open && dirty;
        }
        
        public IEnumerator Wait() {
            while (!IsEmpty())
                yield return null;
        }
        
        public IEnumerator WaitOpen() {
            while (!open)
                yield return null;
        }
        
        class Ticket : IDisposable {
            public MatchingPool pool;

            public void Dispose() {
                pool.CloseTicket(this);
            }
        }
    }
}