DELIMITER //

CREATE PROCEDURE clear_all_tables()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE table_name VARCHAR(255);
    DECLARE cur CURSOR FOR
        SELECT t.table_name
        FROM information_schema.tables t
        WHERE t.table_schema = DATABASE()
          AND t.table_type = 'BASE TABLE'; -- Exclude views
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    SET FOREIGN_KEY_CHECKS = 0;

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO table_name;
        IF done THEN
            LEAVE read_loop;
        END IF;
        SET @sql = CONCAT('TRUNCATE TABLE ', table_name);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;
    CLOSE cur;

    SET FOREIGN_KEY_CHECKS = 1;
END//

DELIMITER ;

-- Run the procedure
CALL clear_all_tables();

-- Clean up
DROP PROCEDURE IF EXISTS clear_all_tables;