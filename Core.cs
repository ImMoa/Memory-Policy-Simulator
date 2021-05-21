using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory_Policy_Simulator
{
    class Core
    {
        private int cursor;
        public int p_frame_size;
        public LinkedList<Page> frame_window; // 큐의 특정 데이터 삭제를 용이하게 하기 위해 연결 리스트로 선언
        public List<Page> pageHistory;
        public LinkedList<PageTime> notUsedTime;
        public LinkedList<PageRef> refCount;
        public int psudoTemp;
        public List<PageRef> psudoTempII;

        public int hit;
        public int fault;
        public int migration;

        public Core(int get_frame_size)
        {
            this.cursor = 0;
            this.p_frame_size = get_frame_size;
            this.frame_window = new LinkedList<Page>();
            this.pageHistory = new List<Page>();
            this.notUsedTime = new LinkedList<PageTime>();
            this.refCount = new LinkedList<PageRef>();
            this.psudoTempII = new List<PageRef>();
        }

        // 특정 인덱스를 연결 리스트에서 삭제할 수 있게 하는 메소드 (Page)
        public LinkedListNode<Page> RemoveAt(LinkedList<Page> frame_window, int index)
        {
            LinkedListNode<Page> currentNode = frame_window.First;
            for (int i = 0; i <= index && currentNode != null; i++)
            {
                if (i != index)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                frame_window.Remove(currentNode);
                return currentNode;
            }
            throw new IndexOutOfRangeException();
        }

        // 특정 인덱스를 연결 리스트에서 삭제할 수 있게 하는 메소드 (PageTime)
        public LinkedListNode<PageTime> RemoveAt(LinkedList<PageTime> frame_window, int index)
        {
            LinkedListNode<PageTime> currentNode = frame_window.First;
            for (int i = 0; i <= index && currentNode != null; i++)
            {
                if (i != index)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                frame_window.Remove(currentNode);
                return currentNode;
            }
            throw new IndexOutOfRangeException();
        }

        // 특정 인덱스를 연결 리스트에서 삭제할 수 있게 하는 메소드 (PageRef)
        public LinkedListNode<PageRef> RemoveAt(LinkedList<PageRef> frame_window, int index)
        {
            LinkedListNode<PageRef> currentNode = frame_window.First;
            for (int i = 0; i <= index && currentNode != null; i++)
            {
                if (i != index)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                frame_window.Remove(currentNode);
                return currentNode;
            }
            throw new IndexOutOfRangeException();
        }

        // 큐와 쿼리 스트링 간 동일한 데이터가 있는지 검사
        public bool IsEquals(LinkedList<Page> frame_window, char[] dataArr)
        {
            for (int i = 0; i < frame_window.Count; i++)
            {
                for (int j = 0; j < dataArr.Length; j++)
                {
                    if (frame_window.ElementAt(i).data.Equals(dataArr[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // 쿼리 스트링을 불러와 가장 멀리 있는 페이지를 찾는 메소드
        public int Predict(LinkedList<Page> frame_window, char[] dataArr)
        {
            char[] queue = new char[frame_window.Count];
            int farthestIndex = 0;
            int res = -1;

            for (int n = 0; n < queue.Length; n++)
                queue[n] = frame_window.ElementAt(n).data;

            for (int i = 0; i < queue.Length; i++)
            {
                for (int j = 0; j < dataArr.Length; j++)
                {
                    if (queue.ElementAt(i).Equals(dataArr[j]))
                    {
                        if (farthestIndex < i)
                        {
                            farthestIndex = i;
                            res = i;
                        }
                    }
                }
            }
            return res;
        }

        // 가장 미사용 시간이 긴 인덱스를 찾는 메소드
        public int searchLongestTime(LinkedList<PageTime> notUsedTime)
        {
            int longest = 0;
            int index = 0;

            for (int i = 0; i < notUsedTime.Count; i++)
            {
                if (longest < notUsedTime.ElementAt(i).notUsedTime)
                {
                    longest = notUsedTime.ElementAt(i).notUsedTime;
                    index = i;
                }
            }
            return index;
        }

        // 가장 참조가 많이 된 프레임을 찾는 메소드
        public int searchMostCount(LinkedList<PageRef> refCount)
        {
            int most = 0;
            int index = 0;

            for (int i = 0; i < refCount.Count; i++)
            {
                if (most < refCount.ElementAt(i).refCount)
                {
                    most = refCount.ElementAt(i).refCount;
                    index = i;
                }
            }
            return index;
        }
        // 가장 참조가 덜 된 프레임을 찾는 메소드
        public int searchLeastCount(LinkedList<PageRef> refCount)
        {
            int least = 999999;
            int index = 0;

            for (int i = 0; i < refCount.Count; i++)
            {
                if (least > refCount.ElementAt(i).refCount)
                {
                    least = refCount.ElementAt(i).refCount;
                    index = i;
                    
                    if (least == 0)
                        break;
                }
            }
            return index;
        }

        public Page.STATUS FIFO(char data)
        {
            Page newPage;
            PageRef refTemp = new PageRef();
            refTemp.refCount = 0;

            if (this.frame_window.Any<Page>(x => x.data == data))
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data) break;
                }
                newPage.loc = i + 1;

            }
            else
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {
                    newPage.status = Page.STATUS.MIGRATION;
                    this.frame_window.RemoveFirst();
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.AddLast(newPage);
            }
            pageHistory.Add(newPage);
            psudoTempII.Add(refTemp);

            return newPage.status;
        }

        public Page.STATUS Opt(char[] dataArr, int n)
        {
            Page newPage;
            PageRef refTemp = new PageRef();

            if (this.frame_window.Any<Page>(x => x.data == dataArr[n]))
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = dataArr[n];
                newPage.status = Page.STATUS.HIT;
                refTemp.refCount = -2;
                this.hit++;
                int i;


                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == dataArr[n]) break;
                }
                newPage.loc = i + 1;
            }
            else
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = dataArr[n];

                if (frame_window.Count >= p_frame_size)
                {
                    if (IsEquals(frame_window, dataArr))
                    {
                        newPage.status = Page.STATUS.MIGRATIONII;

                        // 현재 큐에 존재하는 데이터이면서 쿼리 스트링에서 가장 먼 데이터를 찾음
                        // 그리고 큐에서 해당 인덱스를 삭제함
                        int k = Predict(this.frame_window, dataArr);
                        RemoveAt(this.frame_window, k);
                        refTemp.refCount = k;

                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                    else
                    {
                        newPage.status = Page.STATUS.MIGRATION;
                        refTemp.refCount = 0;
                        this.frame_window.RemoveFirst();
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                }
                else
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    refTemp.refCount = -1;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.AddLast(newPage);
            }
            pageHistory.Add(newPage);
            psudoTempII.Add(refTemp);

            return newPage.status;
        }

        public Page.STATUS Lru(char data)
        {
            Page newPage;
            PageTime timeTemp = new PageTime();
            PageRef refTemp = new PageRef();

            if (this.frame_window.Any<Page>(x => x.data == data))
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data)
                    {
                        notUsedTime.ElementAt(i).notUsedTime = 0;
                        refTemp.refCount = -2;
                        break;
                    }
                }

                for (int n = 0; n < frame_window.Count; n++)
                {
                    notUsedTime.ElementAt(n).notUsedTime++;
                }

                newPage.loc = i + 1;
            }
            else
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {
                    newPage.status = Page.STATUS.MIGRATION;
                    refTemp.refCount = 0;

                    int i = searchLongestTime(notUsedTime);
                    if (i < 1)
                    {
                        frame_window.RemoveFirst();
                        notUsedTime.RemoveFirst();
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                    else
                    {
                        newPage.status = Page.STATUS.MIGRATIONII;
                        refTemp.refCount = i;

                        RemoveAt(frame_window, i);
                        RemoveAt(notUsedTime, i);
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                }
                else
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    refTemp.refCount = -1;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                timeTemp.notUsedTime = 0;

                frame_window.AddLast(newPage);
                notUsedTime.AddLast(timeTemp);

                for (int i = 0; i < notUsedTime.Count; i++)
                {
                    notUsedTime.ElementAt(i).notUsedTime++;
                }
            }
            pageHistory.Add(newPage);
            psudoTempII.Add(refTemp);
            return newPage.status;
        }

        public Page.STATUS Lfu(char data)
        {
            Page newPage;
            PageRef refTemp = new PageRef();

            if (this.frame_window.Any<Page>(x => x.data == data))
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                refTemp.refCount = -2;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data)
                    {   
                        if (refCount.ElementAt(i).refCount >= 1)
                            refCount.ElementAt(i).refCount++;
                        else
                            refCount.ElementAt(i).refCount = 1;
                        break;
                    }
                }

                newPage.loc = i + 1;
            }
            else
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {
                    newPage.status = Page.STATUS.MIGRATION;
                    refTemp.refCount = 0;

                    int i = searchLeastCount(refCount);
                    if (i < 1)
                    {
                        frame_window.RemoveFirst();
                        refCount.RemoveFirst();
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                    else
                    {
                        newPage.status = Page.STATUS.MIGRATIONII;
                        refTemp.refCount = i;

                        RemoveAt(frame_window, i);
                        RemoveAt(refCount, i);
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                }
                else
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    refTemp.refCount = -1;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;

                frame_window.AddLast(newPage);
                refCount.AddLast(refTemp);
            }
            pageHistory.Add(newPage);
            psudoTempII.Add(refTemp);
            return newPage.status;
        }

        public Page.STATUS Mfu(char data)
        {
            Page newPage;
            PageRef refTemp = new PageRef();

            if (this.frame_window.Any<Page>(x => x.data == data))
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                refTemp.refCount = -2;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data)
                    {   
                        if(refCount.ElementAt(i).refCount >= 1)
                            refCount.ElementAt(i).refCount++;
                        else
                            refCount.ElementAt(i).refCount = 1;
                        break;
                    }
                }

                newPage.loc = i + 1;
            }
            else
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {
                    newPage.status = Page.STATUS.MIGRATION;
                    refTemp.refCount = 0;

                    int i = searchMostCount(refCount);
                    if (i < 1)
                    {
                        frame_window.RemoveFirst();
                        refCount.RemoveFirst();
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                    else
                    {
                        newPage.status = Page.STATUS.MIGRATIONII;
                        refTemp.refCount = i;

                        RemoveAt(frame_window, i);
                        RemoveAt(refCount, i);
                        cursor = p_frame_size;
                        this.migration++;
                        this.fault++;
                    }
                }
                else
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    refTemp.refCount = -1;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;

                frame_window.AddLast(newPage);
                refCount.AddLast(refTemp);
            }
            pageHistory.Add(newPage);
            psudoTempII.Add(refTemp);
            return newPage.status;
        }

        public List<Page> GetPageInfo(Page.STATUS status)
        {
            List<Page> pages = new List<Page>();

            foreach (Page page in pageHistory)
            {
                if (page.status == status)
                {
                    pages.Add(page);
                }
            }
            return pages;
        }
    }
}