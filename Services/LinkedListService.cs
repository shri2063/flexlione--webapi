﻿using System.Collections.Generic;
using System.Linq;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;

namespace m_sort_server.Services
{
    public class LinkedListService
    {
        public static LinkedChildTaskHead CreateLinkedList(List<TaskSheetItemEditModel> tasks)
        {
            List<TaskSheetItemEditModel> orderedTasks = tasks.OrderByDescending(x => x.Rank).ToList();

            LinkedChildTaskHead head = new LinkedChildTaskHead()
            {
              
                Pointer = new LinkedChildTask()
                {
                    Task = new TaskSheetItemEditModel(),
                    Next = new LinkedChildTask(),
                    Previous = new LinkedChildTask()
                }
            };

            while (orderedTasks.Count > 0)
            {
                InsertFrontOnHead(head, orderedTasks.FirstOrDefault());
                orderedTasks.RemoveAt(0);
            }
          
            


            return head;
        }

        public static void InsertFrontOnHead(LinkedChildTaskHead head, TaskSheetItemEditModel task)
        {
           
            LinkedChildTask linkedTask = new LinkedChildTask()
            {
                Task = task,
                Next = head.Pointer
            };
          
            head.Pointer = linkedTask;
        }
    }
}