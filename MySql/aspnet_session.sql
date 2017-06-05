SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for aspnet_session
-- ----------------------------
DROP TABLE IF EXISTS `aspnet_session`;
CREATE TABLE `aspnet_session` (
  `SessionID` varchar(50) NOT NULL,
  `Created` datetime NOT NULL COMMENT '创建时间',
  `Expires` datetime NOT NULL COMMENT '过期时间',
  `LockDate` datetime NOT NULL COMMENT '锁定时间',
  `LockId` int(11) NOT NULL,
  `Timeout` int(11) NOT NULL COMMENT '超时时间(分钟)',
  `Locked` bit(1) NOT NULL COMMENT '是否已锁定',
  `SessionItems` varchar(1000) DEFAULT NULL,
  `Flags` int(11) NOT NULL,
  PRIMARY KEY (`SessionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Procedure structure for AddOrUpdateSessionAndUnlock
-- ----------------------------
DROP PROCEDURE IF EXISTS `AddOrUpdateSessionAndUnlock`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `AddOrUpdateSessionAndUnlock`(In SessionID VARCHAR(50),
In LockId int,
In Timeout int,
In SessionItems VARCHAR(1000),In NewItem bit)
    COMMENT '添加session或更新session信息并解锁'
BEGIN
	if NewItem=TRUE THEN
		DELETE FROM aspnet_session where aspnet_session.SessionID=SessionID and aspnet_session.Expires<=NOW();
		insert into aspnet_session(SessionId, Created, Expires, LockDate,LockId, Timeout, Locked, SessionItems, Flags) 
		values(SessionID,NOW(),date_add(NOW(),INTERVAL Timeout MINUTE),NOW(),LockId,Timeout,0,SessionItems,0);
	else 
		UPDATE aspnet_session set Expires=DATE_ADD(NOW(),INTERVAL Timeout MINUTE),SessionItems=SessionItems,
		Locked=0 WHERE SessionID=SessionID AND LockId=LockId;
	end if; 
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for CreateNewSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `CreateNewSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `CreateNewSession`(IN `SessionID` varchar(50),IN `Timeout` int)
    COMMENT '创建新session'
BEGIN
	#Routine body goes here...
	INSERT INTO aspnet_session(SessionID, Created, Expires, LockDate,LockId, Timeout, Locked, SessionItems, Flags)
	Values(SessionID, NOW(), DATE_ADD(NOW(),INTERVAL Timeout MINUTE), NOW(), 0 , Timeout, FALSE, '', 1);
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for DelExpiresSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `DelExpiresSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `DelExpiresSession`()
    COMMENT '删除已过期session'
BEGIN
	#Routine body goes here...
	DELETE FROM aspnet_session where aspnet_session.Expires<NOW();
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for DelSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `DelSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `DelSession`(IN `SessionID` varchar(50),IN `LockId` int)
    COMMENT '删除session'
BEGIN
	#Routine body goes here...
	DELETE FROM aspnet_session where aspnet_session.SessionID=SessionID and aspnet_session.LockId=LockId;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for GetSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `GetSession`(IN `SessionID` varchar(50),IN `LockRecord` bit,OUT `Locked` bit)
    COMMENT '获取session值并锁定session'
BEGIN
	#Routine body goes here...
	DECLARE isExist int;
	if LockRecord=TRUE THEN
		SELECT count(*) into isExist from aspnet_session where SessionID=SessionID1 and Locked=0 and Expires>NOW();
    if isExist>0 THEN
			UPDATE aspnet_session set Locked = 1, LockDate = NOW() where SessionID=SessionID1 and Locked=0 and Expires>NOW();
			set Locked=false;
		ELSE
			set Locked=true;
		end if;
	ELSE
		SELECT NOW(), Expires ,SessionItems, LockId, Flags, Timeout,LockDate, Locked from aspnet_session where SessionID=SessionID and Expires>NOW();
	end if;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for LockSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `LockSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `LockSession`(IN `SessionID` varchar(50),IN `LockId` int)
    COMMENT '锁定session'
BEGIN
	#Routine body goes here...
	update aspnet_session set LockId=LockId where SessionID=SessionID and NOW()<=Expires;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for UnlockSession
-- ----------------------------
DROP PROCEDURE IF EXISTS `UnlockSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `UnlockSession`(IN `Timeout` int,IN `SessionID` varchar(50),IN `LockId` int)
    COMMENT '解除Session'
BEGIN
	#Routine body goes here...
	update aspnet_session set LockId=0,Expires=DATE_ADD(NOW(),INTERVAL Timeout MINUTe) where SessionID=SessionID and LockId=LockId;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for UpdateSessionTimeout
-- ----------------------------
DROP PROCEDURE IF EXISTS `UpdateSessionTimeout`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` PROCEDURE `UpdateSessionTimeout`(IN SessionID varchar(50),In Timeout int)
    COMMENT '更新Session过期时间'
BEGIN
	update aspnet_session set Expires=DATE_ADD(NOW(),INTERVAL Timeout MINUTE) where SessionID=SessionID;
END
;;
DELIMITER ;

-- ----------------------------
-- Event structure for Event_AutoDelExpiresSession
-- ----------------------------
DROP EVENT IF EXISTS `Event_AutoDelExpiresSession`;
DELIMITER ;;
CREATE DEFINER=`root`@`%` EVENT `Event_AutoDelExpiresSession` ON SCHEDULE EVERY 1 MINUTE STARTS '2017-06-05 19:20:00' ON COMPLETION NOT PRESERVE ENABLE COMMENT '自动删除过期session事件' DO CALL DelExpiresSession()
;;
DELIMITER ;
