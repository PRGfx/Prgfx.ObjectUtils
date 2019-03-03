namespace Prgfx.ObjectUtils.Fixtures
{
    class DummyClassWithGettersAndSetters
    {
        protected object property;
        protected object anotherProperty;
        protected object property2;
        protected bool booleanProperty = true;
        protected bool anotherBooleanProperty = false;
        protected object protectedProperty;
        protected string unexposedProperty = "unexposed";
        public object publicProperty;
        public int publicProperty2 = 42;

        public void SetProperty(object value)
        {
            this.property = value;
        }

        public object GetProperty()
        {
            return this.property;
        }

        public void SetAnotherProperty(object value)
        {
            this.anotherProperty = value;
        }

        public object GetAnotherProperty()
        {
            return this.anotherProperty;
        }

        protected string GetProtectedProperty()
        {
            return "42";
        }

        protected void SetProtectedProperty(object value)
        {
            this.protectedProperty = value;
        }

        public string IsBooleanProperty()
        {
            return "method called " + this.booleanProperty;
        }

        public void SetAnotherBooleanProperty(bool value)
        {
            this.anotherBooleanProperty = value;
        }

        public bool HasAnotherBooleanProperty()
        {
            return this.anotherBooleanProperty;
        }

        protected string GetPrivateProperty()
        {
            return "21";
        }

        /* public void SetWriteOnlyMagicProperty(object value)
        {
        } */
    }
}