﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#if UNITY
using TypeInfo = System.Type;
#endif

namespace Parse.Internal {
  internal class ObjectSubclassingController : IObjectSubclassingController {
    private readonly ReaderWriterLockSlim mutex;
    private readonly IDictionary<String, ObjectSubclassInfo> registeredSubclasses;
    private IDictionary<String, Action> registerActions;

    public ObjectSubclassingController(IDictionary<Type, Action> actions) {
      mutex = new ReaderWriterLockSlim();
      registeredSubclasses = new Dictionary<String, ObjectSubclassInfo>();
      registerActions = actions.ToDictionary(p => GetClassName(p.Key), p => p.Value);
    }

    public String GetClassName(Type type) {
      return ObjectSubclassInfo.GetClassName(type.GetTypeInfo());
    }

    public Type GetType(String className) {
      ObjectSubclassInfo info = null;
      mutex.EnterReadLock();
      registeredSubclasses.TryGetValue(className, out info);
      mutex.ExitReadLock();

      return info != null
        ? info.TypeInfo.AsType()
        : null;
    }

    public bool IsTypeValid(String className, Type type) {
      ObjectSubclassInfo subclassInfo = null;

      mutex.EnterReadLock();
      registeredSubclasses.TryGetValue(className, out subclassInfo);
      mutex.ExitReadLock();

      return subclassInfo == null
        ? type == typeof(ParseObject)
        : subclassInfo.TypeInfo == type.GetTypeInfo();
    }

    public void RegisterSubclass(Type type) {
      TypeInfo typeInfo = type.GetTypeInfo();
      if (!typeInfo.IsSubclassOf(typeof(ParseObject))) {
        throw new ArgumentException("Cannot register a type that is not a subclass of ParseObject");
      }

      String className = ObjectSubclassInfo.GetClassName(typeInfo);

      try {
        // Perform this as a single independent transaction, so we can never get into an
        // intermediate state where we *theoretically* register the wrong class due to a
        // TOCTTOU bug.
        mutex.EnterWriteLock();

        ObjectSubclassInfo previousInfo = null;
        if (registeredSubclasses.TryGetValue(className, out previousInfo)) {
          if (typeInfo.IsAssignableFrom(previousInfo.TypeInfo)) {
            // Previous subclass is more specific or equal to the current type, do nothing.
            return;
          } else if (previousInfo.TypeInfo.IsAssignableFrom(typeInfo)) {
            // Previous subclass is parent of new child, fallthrough and actually register
            // this class.
            /* Do nothing */
          } else {
            throw new ArgumentException(
              "Tried to register both " + previousInfo.TypeInfo.FullName + " and " + typeInfo.FullName +
              " as the ParseObject subclass of " + className + ". Cannot determine the right class " +
              "to use because neither inherits from the other."
            );
          }
        }

        ConstructorInfo constructor = type.FindConstructor();
        if (constructor == null) {
          throw new ArgumentException("Cannot register a type that does not implement the default constructor!");
        }

        registeredSubclasses[className] = new ObjectSubclassInfo(type, constructor);
      } finally {
        mutex.ExitWriteLock();
      }

      Action toPerform = null;
      if (registerActions.TryGetValue(className, out toPerform)) {
        toPerform();
      }
    }

    public void UnregisterSubclass(Type type) {
      mutex.EnterWriteLock();
      registeredSubclasses.Remove(GetClassName(type));
      mutex.ExitWriteLock();
    }

    public ParseObject Instantiate(String className) {
      ObjectSubclassInfo info = null;

      mutex.EnterReadLock();
      registeredSubclasses.TryGetValue(className, out info);
      mutex.ExitReadLock();

      return info != null
        ? info.Instantiate()
        : new ParseObject(className);
    }

    public IDictionary<String, String> GetPropertyMappings(String className) {
      ObjectSubclassInfo info = null;
      mutex.EnterReadLock();
      registeredSubclasses.TryGetValue(className, out info);
      mutex.ExitReadLock();

      return info != null
        ? info.PropertyMappings
        : null;
    }
  }
}
