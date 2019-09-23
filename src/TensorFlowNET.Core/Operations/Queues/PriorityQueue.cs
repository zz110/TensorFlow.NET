﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Tensorflow.Binding;

namespace Tensorflow.Queues
{
    public class PriorityQueue : QueueBase
    {
        public PriorityQueue(int capacity,
            TF_DataType[] dtypes,
            TensorShape[] shapes,
            string[] names = null,
            string shared_name = null,
            string name = "priority_queue")
        : base(dtypes: dtypes, shapes: shapes, names: names)
        {
            _queue_ref = gen_data_flow_ops.priority_queue_v2(
                component_types: dtypes,
                shapes: shapes,
                capacity: capacity,
                shared_name: shared_name,
                name: name);

            _name = _queue_ref.op.name.Split('/').Last();

            var dtypes1 = dtypes.ToList();
            dtypes1.Insert(0, TF_DataType.TF_INT64);
            _dtypes = dtypes1.ToArray();

            var shapes1 = shapes.ToList();
            shapes1.Insert(0, new TensorShape());
            _shapes = shapes1.ToArray();
        }

        public Operation enqueue_many<T>(long[] indexes, T[] vals, string name = null)
        {
            return tf_with(ops.name_scope(name, $"{_name}_EnqueueMany", vals), scope =>
            {
                var vals_tensor1 = _check_enqueue_dtypes(indexes); 
                var vals_tensor2 = _check_enqueue_dtypes(vals);

                var tensors = new List<Tensor>();
                tensors.AddRange(vals_tensor1);
                tensors.AddRange(vals_tensor2);

                return gen_data_flow_ops.queue_enqueue_many_v2(_queue_ref, tensors.ToArray(), name: scope);
            });
        }

        public Tensor[] dequeue(string name = null)
        {
            Tensor[] ret;
            if (name == null)
                name = $"{_name}_Dequeue";

            if (_queue_ref.dtype == TF_DataType.TF_RESOURCE)
                ret = gen_data_flow_ops.queue_dequeue_v2(_queue_ref, _dtypes, name: name);
            else
                ret = gen_data_flow_ops.queue_dequeue(_queue_ref, _dtypes, name: name);

            return ret;
        }
    }
}
