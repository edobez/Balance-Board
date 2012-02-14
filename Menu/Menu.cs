using System;
using System.Collections;
using Microsoft.SPOT;

namespace Menu
{
    class MenuManager
    {
        /// <summary>
        /// Root menu
        /// </summary>
        private MenuItem _rootMenu;
        private int _currentMenuIndex;
        private int _currentSubMenuIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        public MenuManager()
        {
            _rootMenu = new MenuItem("Root");
        }

        public MenuItem Root
        {
            get { return _rootMenu; }
        }

        /// <summary>
        /// Handler of button Enter pressed
        /// </summary>
        public void butEnter()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Handler of button Menu pressed
        /// </summary>
        public void butMenu()
        {
            _currentMenuIndex++;
            if (_currentMenuIndex > Root.SubMenuCount) _currentMenuIndex = 0;
        }

        /// <summary>
        /// Handler of button Up pressed
        /// </summary>
        public void butUp()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Handler of button Down pressed
        /// </summary>
        public void butDown()
        {
            throw new System.NotImplementedException();
        }

        public void display()
        {
            MenuItem currentMenu = Root.getChild(_currentMenuIndex);
            MenuItem currentSubMenu = Root.getChild(_currentMenuIndex).getChild(_currentSubMenuIndex);

            Debug.Print(currentMenu.Name);
            if (currentSubMenu.Param.Length > 0) Debug.Print(currentSubMenu.Name + " " + currentSubMenu.Param);
            else Debug.Print(currentSubMenu.Name);
        }
    }

    public class MenuItem
    {
        private ArrayList _childMenu;
        private MenuItem _parentMenu;
        private Object _param;
        private String _name;
        private int _level;
        private bool _isEditable;

        /// <summary>
        /// Constructor for menu
        /// </summary>
        /// <param item="item">Name of the menu</param>
        /// <param item="param">Object to show</param>
        /// <param item="editable">Tells if the object is editable</param>
        public MenuItem(string name, object param, bool editable)
        {
            _childMenu = new ArrayList();
            _name = name;
            _param = param;
            _isEditable = editable;
        }

        /// <summary>
        /// Constructor for name-only menu
        /// </summary>
        /// <param item="item">Name of the menu</param>
        public MenuItem(string name) : this(name, null, false)
        {
        }

        /// <summary>
        /// Constructor for root
        /// </summary>
        public MenuItem() : this("Root")
        {
            _level = 0;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int SubMenuCount
        {
            get { return _childMenu.Count; }
        }

        public string Param
        {
            get { return _param.ToString(); }
        }

        public MenuItem addMenu(MenuItem item)
        {
            _childMenu.Add(item);
            item._parentMenu = this;
            item._level = this._level + 1;
            return this;
        }

        public MenuItem getChild(int index)
        {
            return (_childMenu[index] as MenuItem);
        }
    }
}
