using System;
using System.Collections;
using Microsoft.SPOT;

namespace Menu
{
    class MenuManager
    {
        private int m_currentMenuIndex;
        private int m_currentSubMenuIndex;
        private MenuItem m_currentMenu;
        private MenuItem m_currentSubMenu;
        private Hashtable m_menuList;
        private int m_menuCount;
        private int[] m_subMenuCount;

        public MenuManager()
        {
            m_menuList = new Hashtable();
            m_currentMenuIndex = 1;
            m_currentSubMenuIndex = 1;
            m_subMenuCount = new int[10];
        }

        public void addMenu(MenuItem item, int id)
        {
            m_menuList.Add(id, item);
            m_menuCount++;
        }

        public void addSubMenu(MenuItem item, int id)
        {
            int temp = id / 10;
            m_menuList.Add(id, item);
            m_subMenuCount[temp]++;
        }

        public void display()
        {
            int id = m_currentMenuIndex * 10 + m_currentSubMenuIndex;
            int id_top = m_currentMenuIndex * 10;

            if (m_menuList.Contains(id))
            {
                MenuItem item_top = m_menuList[id_top] as MenuItem;
                MenuItem item = m_menuList[id] as MenuItem;

                Debug.Print(item_top.Name);
                Debug.Print(item.Name);
            }
        }
    }

    public class MenuItem
    {
        private Object m_param;
        private String m_name;
        private bool m_isEditable;

        public MenuItem(string name, object param, bool isEditable)
        {
            m_name = name;
            m_param = param;
            m_isEditable = isEditable;
        }

        public MenuItem(string name)
            : this(name, null, false)
        { }

        public string Name
        {
            get { return m_name; }
        }

        public object Param
        {
            get { return m_param; }
        }
    }
}
