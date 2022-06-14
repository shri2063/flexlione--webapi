using System.Collections.Generic;
using System.Linq;
using m_sort_server.EditModels;
using m_sort_server.LinkedListModel;

namespace m_sort_server.Services
{
    public class LinkedListService
    {
        public static LinkedChildTaskHead CreateLinkedList(List<TaskDetailEditModel> tasks)
        {
            List<TaskDetailEditModel> orderedTasks = tasks.OrderByDescending(x => x.Rank).ToList();

            LinkedChildTaskHead head = new LinkedChildTaskHead()
            {
              
                Pointer = new LinkedChildTask()
                {
                    TaskDetail = new TaskDetailEditModel(),
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

        public static void InsertFrontOnHead(LinkedChildTaskHead head, TaskDetailEditModel taskDetail)
        {
           
            LinkedChildTask linkedTask = new LinkedChildTask()
            {
                TaskDetail = taskDetail,
                Next = head.Pointer
            };
          
            head.Pointer = linkedTask;
        }
    }
}