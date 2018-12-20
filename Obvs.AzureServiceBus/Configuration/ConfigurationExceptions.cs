using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Obvs.AzureServiceBus.Configuration {
    /// <summary>
    /// MappingAlreadyExistsForMessageTypeException
    /// </summary>
    [Serializable]
    public class MappingAlreadyExistsForMessageTypeException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="entityType"></param>
        public MappingAlreadyExistsForMessageTypeException(Type messageType, MessagingEntityType entityType) : base(string.Format("A mapping already exists for message type {0} for entity type {1}", messageType.Name, entityType)) {
            MessageType = messageType;
            EntityType = entityType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MappingAlreadyExistsForMessageTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Type of message
        /// </summary>
        /// <value></value>
        public Type MessageType {
            get;
            private set;
        }

        /// <summary>
        /// Messaging entity type
        /// </summary>
        /// <value></value>
        public MessagingEntityType EntityType {
            get;
            private set;
        }
    }

    /// <summary>
    /// Ambigous type mapping exception
    /// </summary>
    [Serializable]
    public class AmbiguosMessageTypeMappingException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="expectedEntityTypes"></param>
        public AmbiguosMessageTypeMappingException(Type messageType, IEnumerable<MessagingEntityType> expectedEntityTypes) : base(string.Format("More than one mapping exists for message type {0} for expected entity types {1}", messageType.Name, string.Join(", ", expectedEntityTypes))) {
            MessageType = messageType;
            ExpectedEntityTypes = expectedEntityTypes;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected AmbiguosMessageTypeMappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Type of message
        /// </summary>
        public Type MessageType {
            get;
            private set;
        }

        /// <summary>
        /// Expected entity types
        /// </summary>
        public IEnumerable<MessagingEntityType> ExpectedEntityTypes {
            get;
            private set;
        }
    }

    /// <summary>
    /// MessagingEntityDoesNotAlreadyExistException
    /// </summary>
    [Serializable]
    public class MessagingEntityDoesNotAlreadyExistException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="messagingEntityType"></param>
        public MessagingEntityDoesNotAlreadyExistException(string path, MessagingEntityType messagingEntityType) : base(string.Format("A messaging entity with a path of \"{0}\" of type {1} does not exist and was not configured to be created automatically.", path, messagingEntityType)) {
            Path = path;
            MessagingEntityType = messagingEntityType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MessagingEntityDoesNotAlreadyExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Path
        /// </summary>
        public string Path {
            get;
            private set;
        }

        /// <summary>
        /// Messaging entity type
        /// </summary>
        /// <value></value>
        public MessagingEntityType MessagingEntityType {
            get;
            private set;
        }
    }

    /// <summary>
    /// MessagingEntityAlreadyExistsException
    /// </summary>
    [Serializable]
    public class MessagingEntityAlreadyExistsException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="messagingEntityType"></param>
        public MessagingEntityAlreadyExistsException(string path, MessagingEntityType messagingEntityType) : base(string.Format("A messaging entity with a path of \"{0}\" of type {1} already exists. To ensure intent and keep your data safe the framwork will not recreate it as temporary unless explicitly configured to do so. You can change the configuration to explicitly enable deletion of existing temporary entities or manually delete the entity.", path, messagingEntityType)) {
            Path = path;
            MessagingEntityType = messagingEntityType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MessagingEntityAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Path
        /// </summary>
        public string Path {
            get;
            private set;
        }

        /// <summary>
        /// Messaging entity type
        /// </summary>
        /// <value></value>
        public MessagingEntityType MessagingEntityType {
            get;
            private set;
        }
    }
}
