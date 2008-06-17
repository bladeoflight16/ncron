﻿// Copyright (c) 2007-2008, Joern Schou-Rode <jsr@malamute.dk>
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.
// 
// * Neither the name of the NCron nor the names of its contributors may
//   be used to endorse or promote products derived from this software
//   without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace NCron.Scheduling
{
    public class Plan : IEnumerable<DateTime>
    {
        #region Public properties and constructors

        public PatternCollection Minutes { get; private set; }
        public PatternCollection Hours { get; private set; }
        public PatternCollection DaysOfWeek { get; private set; }
        public PatternCollection DaysOfMonth { get; private set; }
        public PatternCollection Months { get; private set; }

        public Plan()
        {
            this.Minutes = new PatternCollection();
            this.Hours = new PatternCollection();
            this.DaysOfWeek = new PatternCollection();
            this.DaysOfMonth = new PatternCollection();
            this.Months = new PatternCollection();
        }

        #endregion

        #region Algorithm for computation of new execution DateTime

        public DateTime ComputeNextExecution(DateTime previous)
        {
            bool force = true;

            Queue<Field> workList = new Queue<Field>();
            workList.Enqueue(Field.Minute);
            if (previous == DateTime.MinValue)
            {
                workList.Enqueue(Field.Hour);
                workList.Enqueue(Field.Day);
                workList.Enqueue(Field.Month);
                force = false;
            }

            while (workList.Count > 0)
            {
                DateTime dt = previous;
                switch (workList.Dequeue())
                {
                    case Field.Minute:
                        dt = previous.AddMinutes(Minutes.ComputeOffset(dt.Minute, 60, force));
                        if (dt.Hour != previous.Hour) workList.Enqueue(Field.Hour);
                        force = false;
                        break;

                    case Field.Hour:
                        dt = dt.AddHours(Minutes.ComputeOffset(dt.Hour, 24, false));
                        if (dt.Hour != previous.Hour)
                        {
                            if (dt.Day != previous.Day) workList.Enqueue(Field.Day);
                            dt = dt.AddMinutes(-dt.Minute);
                            workList.Enqueue(Field.Minute);
                        }
                        break;

                    case Field.Day:
                        int dow = DaysOfWeek.ComputeOffset((int)dt.DayOfWeek, 7, false);
                        int dom = DaysOfMonth.ComputeOffset(dt.Day, DateTime.DaysInMonth(dt.Year, dt.Month), false);
                        dt = dt.AddDays(dow < dom ? dow : dom);
                        if (dt.Day != previous.Day)
                        {
                            if (dt.Month != previous.Month) workList.Enqueue(Field.Month);
                            dt = dt.AddHours(-dt.Hour).AddMinutes(-dt.Minute);
                            workList.Enqueue(Field.Hour);
                            workList.Enqueue(Field.Minute);
                        }
                        break;

                    case Field.Month:
                        dt = dt.AddMonths(Months.ComputeOffset(dt.Month, 12, false));
                        if (dt.Month != previous.Month)
                        {
                            dt = dt.AddDays(-dt.Day).AddHours(-dt.Hour).AddMinutes(-dt.Minute);
                            workList.Enqueue(Field.Day);
                            workList.Enqueue(Field.Hour);
                            workList.Enqueue(Field.Minute);
                        }
                        break;
                }
                previous = dt;
            }

            return previous;
        }

        private enum Field { Minute, Hour, Day, Month }

        #endregion

        #region IEnumerable and IEnumerable<DateTime> implementation

        public IEnumerator<DateTime> GetEnumerator(DateTime start)
        {
            return new PlanEnumerator(this, start);
        }

        public IEnumerator<DateTime> GetEnumerator()
        {
            return GetEnumerator(DateTime.Now);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(DateTime.Now);
        }

        private class PlanEnumerator : IEnumerator<DateTime>
        {
            public PlanEnumerator(Plan plan, DateTime start)
            {
            }

            public DateTime Current
            {
                get { throw new NotImplementedException(); }
            }

            object IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}