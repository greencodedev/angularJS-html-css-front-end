CREATE TABLE Categories (
   CategoryId int PRIMARY KEY AUTO_INCREMENT,
   AttrName varchar(20) NOT NULL,
   Label varchar(250) NOT NULL,
   SortOrder int NOT NULL
);

CREATE TABLE Products (
   ProductId int PRIMARY KEY AUTO_INCREMENT,
   CategoryId int NOT NULL REFERENCES Categories(CategoryId),
   FinaleId varchar(250) NOT NULL,
   Name varchar(250) NULL,
   Description varchar(2048) NULL
 );

ALTER TABLE Categories
ADD Status int not null DEFAULT 1;


 ALTER TABLE Products
ADD FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId);


ALTER TABLE Products modify CategoryId int null;

Alter table Products ADD UNIQUE (AttrName);

-- -----------------------------------------------------
-- Table `emp_phones`.`PrinterSizes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `emp_phones`.`PrinterSizes` (
  `PrinterSizeId` INT NOT NULL AUTO_INCREMENT,
  `Zpl` VARCHAR(4096) NULL,
  `Name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`PrinterSizeId`),
  UNIQUE INDEX `PrinterSizeId_UNIQUE` (`PrinterSizeId` ASC),
  UNIQUE INDEX `Name_UNIQUE` (`Name` ASC))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `emp_phones`.`Printers`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `emp_phones`.`Printers` (
  `PrinterId` INT NOT NULL AUTO_INCREMENT,
  `PrinterSizeId` INT NULL,
  `Id` VARCHAR(45) NOT NULL,
  `Name` VARCHAR(256) NOT NULL,
  `Description` VARCHAR(1024) NULL,
  `State` VARCHAR(45) NULL,
  PRIMARY KEY (`PrinterId`),
  UNIQUE INDEX `PrinterId_UNIQUE` (`PrinterId` ASC),
  UNIQUE INDEX `Id_UNIQUE` (`Id` ASC),
  INDEX `printers_printersize_idx` (`PrinterSizeId` ASC),
  CONSTRAINT `printers_printersize`
    FOREIGN KEY (`PrinterSizeId`)
    REFERENCES `emp_phones`.`PrinterSizes` (`PrinterSizeId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `emp_phones`.`PrintJobs`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `emp_phones`.`PrintJobs` (
  `PrintJobId` INT NOT NULL AUTO_INCREMENT,
  `PrinterId` INT NOT NULL,
  `ProductId` INT NOT NULL,
  `Status` VARCHAR(256) NOT NULL,
  PRIMARY KEY (`PrintJobId`),
  UNIQUE INDEX `PrintJobId_UNIQUE` (`PrintJobId` ASC),
  INDEX `print_job_printer_idx` (`PrinterId` ASC),
  INDEX `print_job_product_idx` (`ProductId` ASC),
  CONSTRAINT `print_job_printer`
    FOREIGN KEY (`PrinterId`)
    REFERENCES `emp_phones`.`Printers` (`PrinterId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `print_job_product`
    FOREIGN KEY (`ProductId`)
    REFERENCES `emp_phones`.`Products` (`ProductId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;

ALTER TABLE PrintJobs
ADD PrintNodePrintJobId INT null AFTER PrintJobId,
ADD UserId VARCHAR(128) not null AFTER PrintJobId,
ADD Quantity INT not null DEFAULT 1 AFTER ProductId,
ADD Created DATETIME not null DEFAULT CURRENT_TIMESTAMP;

/* Up to here */

CREATE TABLE Conditions (
   ConditionId INT PRIMARY KEY AUTO_INCREMENT,
   Code VARCHAR(225),
   Name VARCHAR(1024),
   Description VARCHAR(2048),
   Convertible BIT
 );

ALTER TABLE Conditions ADD UNIQUE (Name)

 ALTER TABLE Products 
 ADD ConditionId INT,
 ADD FOREIGN KEY product_condition(ConditionId) REFERENCES Conditions(ConditionId);

ALTER TABLE Products 
ADD Ncs VARCHAR(512);

CREATE UNIQUE INDEX lookup_name_value ON Lookup (LookupType, LookupValue);

CREATE UNIQUE INDEX prod_ncs_cond ON Products (Ncs, ConditionId);

CREATE TABLE `UserPrinters` (
	`UserPrinterId` INT(11) NOT NULL AUTO_INCREMENT,
	`UserId` VARCHAR(128) NOT NULL,
	`PrinterId` INT(11) NOT NULL,
	PRIMARY KEY (`UserPrinterId`),
	UNIQUE INDEX `UserPrinter` (`UserId`, `PrinterId`),
	INDEX `userprinters_printers` (`PrinterId`),
	CONSTRAINT `userprinters_printers` FOREIGN KEY (`PrinterId`) REFERENCES `Printers` (`PrinterId`)
)
COLLATE='latin1_swedish_ci'
ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `emp_phones`.`Conversions` (
  `ConversionId` INT NOT NULL AUTO_INCREMENT,
  `FromProductId` INT NOT NULL,
  `ToProductId` INT NOT NULL,
  `FromSublocationId` INT NOT NULL,
  `ToSublocationId` INT NOT NULL,
  `Quantity` INT not NULL,
  `PoNumber` VARCHAR(128) NULL,
  `Created` DATETIME not null DEFAULT CURRENT_TIMESTAMP,
  `UserId` VARCHAR(128) not null,
  PRIMARY KEY (`ConversionId`),
  UNIQUE INDEX `ConversionId_UNIQUE` (`ConversionId` ASC),
  INDEX `conversion_from_product_idx` (`FromProductId` ASC),
  INDEX `conversion_to_product_idx` (`ToProductId` ASC),
  INDEX `conversion_from_Sublocation_idx` (`FromSublocationId` ASC),
  INDEX `conversion_to_Sublocation_idx` (`ToSublocationId` ASC),
  INDEX `conversion_user_idx` (`UserId` ASC),
  CONSTRAINT `conversion_from_product`
    FOREIGN KEY (`FromProductId`)
    REFERENCES `emp_phones`.`Products` (`ProductId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `conversion_to_product`
    FOREIGN KEY (`ToProductId`)
    REFERENCES `emp_phones`.`Products` (`ProductId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `conversion_from_sublocation`
    FOREIGN KEY (`FromSublocationId`)
    REFERENCES `emp_phones`.`Sublocations` (`SublocationId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `conversion_to_sublocation`
    FOREIGN KEY (`ToSublocationId`)
    REFERENCES `emp_phones`.`Sublocations` (`SublocationId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;

ALTER TABLE Conditions 
ADD Status VARCHAR(128);


/* Because it won't allow to insert if exists but deleted
ALTER TABLE `Products`
	DROP INDEX `prod_ncs_cond`;*/


/*  */

alter table Conditions 
modify column Convertible bit(1) not null default 0;

/*  */

ALTER TABLE Conversions
ADD NewProduct bit(1) not null DEFAULT 0;

ALTER TABLE Sublocations 
CHANGE `Convertible` `ConvertibleFrom` BIT(1) NOT NULL DEFAULT 0;

ALTER TABLE Sublocations 
ADD ConvertibleTo BIT(1) NOT NULL DEFAULT 0;

/* EMB-109 */
ALTER TABLE Conversions
ADD ConvertType VARCHAR(128) NULL;

/* EMB-110 */
ALTER TABLE Conversions
ADD Note VARCHAR(2048) NULL;

/* EMB-107 */
CREATE TABLE IF NOT EXISTS `UserLocations` (
  `UserLocationId` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  `SublocationId` int(11) NOT NULL,
  `ConvertTo` bit(1) NOT NULL DEFAULT b'0',
  `ConvertFrom` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`UserLocationId`),
  UNIQUE KEY `UserLocation` (`UserId`,`SublocationId`),
  KEY `userlocations_locations` (`SublocationId`),
  CONSTRAINT `userlocations_locations` FOREIGN KEY (`SublocationId`) REFERENCES `Sublocations` (`SublocationId`)
)

CREATE TABLE IF NOT EXISTS `UserConditions` (
  `UserConditionId` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  `ConditionId` int(11) NOT NULL,
  `ConvertTo` bit(1) NOT NULL DEFAULT b'0',
  `ConvertFrom` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`UserConditionId`),
  UNIQUE KEY `UserCondition` (`UserId`,`ConditionId`),
  KEY `userconditions_conditions` (`ConditionId`),
  CONSTRAINT `userconditions_conditions` FOREIGN KEY (`ConditionId`) REFERENCES `Conditions` (`ConditionIdUserConditions`)
)




alter table Conversions
add FromProductFinaleId varchar(250),
add FromProductName varchar(250),
add FromProductConditionCode varchar(250),
add FromProductCategoryName varchar(250),
add ToProductFinaleId varchar(250),
add ToProductName varchar(250),
add ToProductConditionCode varchar(250),
add ToProductCategoryName varchar(250);