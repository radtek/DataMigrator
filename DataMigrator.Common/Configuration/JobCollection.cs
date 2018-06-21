﻿using System;
using System.Linq;
using DataMigrator.Common.Collections;

namespace DataMigrator.Common.Configuration
{
    public class JobCollection : StandardList<Job>
    {
        public Job this[string name]
        {
            get { return this.SingleOrDefault(j => j.Name == name); }
        }

        public new void Add(Job job)
        {
            if (this[job.Name] != null)
            {
                throw new ArgumentException(string.Concat("The job, '", job.Name, "' already exists!", "job"));
            }
            base.Add(job);
        }
    }
}