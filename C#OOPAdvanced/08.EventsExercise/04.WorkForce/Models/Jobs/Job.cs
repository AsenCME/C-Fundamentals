﻿namespace _04.WorkForce
{
    using System;
    using _04.WorkForce.Models;

    public class Job
    {
        public event Startup.JobDoneEventHandler JobDone;

        public Job(string name, int workHoursRequired, Employee emploee)
        {
            this.Name = name;
            this.WorkHoursRequired = workHoursRequired;
            this.Emploee = emploee;
            this.IsDone = false;
        }

        public string Name { get; private set; }
        public int WorkHoursRequired { get; private set; }
        public Employee Emploee { get; private set; }
        public bool IsDone { get; private set; }

        public void Update()
        {
            this.WorkHoursRequired -= Emploee.WorkHoursPerWeek;
            if (this.WorkHoursRequired <= 0 && !this.IsDone)
            {
                if (JobDone != null)
                {
                    JobDone(this, new JobEventArgs(this));
                }
            }
        }

        public void OnJobDone(object sebder, JobEventArgs e)
        {
            Console.WriteLine($"Job {this.Name} done!");
            IsDone = true;
        }

        public override string ToString()
        {
            return $"Job: {this.Name} Hours Remaining: {this.WorkHoursRequired}";
        }
    }
}