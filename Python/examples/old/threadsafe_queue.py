from threading import Lock, Semaphore
from queue import Queue

class threadsafe_queue:

    def __init__(self):
        self.sem = Semaphore(0)
        self.queue = Queue()
        self.lock = Lock()

    # returns None if there isn't anything
    def get(self):
        self.lock.acquire()
        ret = None
        if self.queue.qsize() > 0:
            self.sem.acquire()
            ret = self.queue.get()
        self.lock.release()
        return ret

    #does not return 'None', instead waits for the next item
    def get_blocking(self):
        self.sem.acquire()
        self.lock.acquire()
        ret = self.queue.get()
        self.lock.release()
        return ret


    def put(self, item):
        self.lock.acquire()
        self.queue.put(item)
        self.sem.release()
        self.lock.release()

    def size(self):
        self.lock.acquire()
        ret = self.queue.qsize()
        self.lock.release()
        return ret