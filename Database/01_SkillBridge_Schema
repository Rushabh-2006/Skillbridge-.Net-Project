-- SkillBridge MySQL Schema Script (01_SkillBridge_Schema.sql)

CREATE DATABASE IF NOT EXISTS `skillbridge_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `skillbridge_db`;

-- Drop existing tables in reverse dependency order
DROP TABLE IF EXISTS `VerificationBadges`;
DROP TABLE IF EXISTS `Mentorships`;
DROP TABLE IF EXISTS `Applications`;
DROP TABLE IF EXISTS `Internships`;
DROP TABLE IF EXISTS `Certifications`;
DROP TABLE IF EXISTS `StudentProjects`;
DROP TABLE IF EXISTS `Projects`;
DROP TABLE IF EXISTS `RoadmapLevels`;
DROP TABLE IF EXISTS `CareerSkills`;
DROP TABLE IF EXISTS `CareerGoals`;
DROP TABLE IF EXISTS `StudentSkills`;
DROP TABLE IF EXISTS `Skills`;
DROP TABLE IF EXISTS `StudentProfiles`;
DROP TABLE IF EXISTS `Companies`;
DROP TABLE IF EXISTS `Users`;

-- 1. Users Table
CREATE TABLE `Users` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(150) NOT NULL,
    `Email` VARCHAR(150) NOT NULL UNIQUE,
    `PasswordHash` VARCHAR(255) NOT NULL,
    `Role` VARCHAR(50) NOT NULL, -- Student, Recruiter, Faculty, Admin
    `PhoneNumber` VARCHAR(50) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Companies Table
CREATE TABLE `Companies` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `CompanyName` VARCHAR(150) NOT NULL,
    `Industry` VARCHAR(100) NULL,
    `Website` VARCHAR(255) NULL,
    `Location` VARCHAR(150) NULL,
    `Description` TEXT NULL,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Student Profiles
CREATE TABLE `StudentProfiles` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL UNIQUE,
    `Bio` TEXT NULL,
    `College` VARCHAR(150) NULL,
    `Degree` VARCHAR(100) NULL,
    `GraduationYear` INT NULL,
    `TargetCareerGoalId` INT NULL,
    `ReadinessScore` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    `LinkedInUrl` VARCHAR(255) NULL,
    `GitHubUrl` VARCHAR(255) NULL,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Skills Master Table
CREATE TABLE `Skills` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(100) NOT NULL UNIQUE,
    `Category` VARCHAR(100) NOT NULL,
    `Description` TEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Student Skills Table (with Evidence flags)
CREATE TABLE `StudentSkills` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `StudentId` INT NOT NULL,
    `SkillId` INT NOT NULL,
    `ConfidencePercentage` INT NOT NULL DEFAULT 50, -- 1-100%
    `HasProjectEvidence` BOOLEAN NOT NULL DEFAULT FALSE,
    `HasCertificateEvidence` BOOLEAN NOT NULL DEFAULT FALSE,
    `HasAssessmentEvidence` BOOLEAN NOT NULL DEFAULT FALSE,
    `HasGitHubEvidence` BOOLEAN NOT NULL DEFAULT FALSE,
    `HasFacultyVerification` BOOLEAN NOT NULL DEFAULT FALSE,
    `VerificationStatus` VARCHAR(50) NOT NULL DEFAULT 'UNVERIFIED', -- VERIFIED / UNVERIFIED
    FOREIGN KEY (`StudentId`) REFERENCES `StudentProfiles`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`SkillId`) REFERENCES `Skills`(`Id`) ON DELETE CASCADE,
    UNIQUE KEY `uk_student_skill` (`StudentId`, `SkillId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Career Goals Master Table
CREATE TABLE `CareerGoals` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Title` VARCHAR(150) NOT NULL UNIQUE,
    `Description` TEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. Career Required Skills Mapping
CREATE TABLE `CareerSkills` (
    `CareerGoalId` INT NOT NULL,
    `SkillId` INT NOT NULL,
    PRIMARY KEY (`CareerGoalId`, `SkillId`),
    FOREIGN KEY (`CareerGoalId`) REFERENCES `CareerGoals`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`SkillId`) REFERENCES `Skills`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Roadmap Levels Table (Mapping Skills to Levels 1-5 for a Career Goal)
CREATE TABLE `RoadmapLevels` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CareerGoalId` INT NOT NULL,
    `LevelNumber` INT NOT NULL, -- 1 to 5
    `LevelTitle` VARCHAR(150) NOT NULL,
    `SkillId` INT NOT NULL,
    `Description` TEXT NULL,
    FOREIGN KEY (`CareerGoalId`) REFERENCES `CareerGoals`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`SkillId`) REFERENCES `Skills`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Projects Master Table
CREATE TABLE `Projects` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Title` VARCHAR(150) NOT NULL,
    `Difficulty` VARCHAR(50) NOT NULL, -- Beginner, Intermediate, Advanced, Expert
    `Description` TEXT NULL,
    `RequiredSkillIds` VARCHAR(255) NOT NULL -- Comma-separated Skill IDs
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10. Student Projects (Build & Upload Proof)
CREATE TABLE `StudentProjects` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `StudentId` INT NOT NULL,
    `ProjectId` INT NOT NULL,
    `RepositoryUrl` VARCHAR(255) NULL,
    `LiveDemoUrl` VARCHAR(255) NULL,
    `ProofFilePath` VARCHAR(255) NULL,
    `SubmittedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Submitted', -- Submitted, Reviewed
    FOREIGN KEY (`StudentId`) REFERENCES `StudentProfiles`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 11. Certifications Upload Table
CREATE TABLE `Certifications` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `StudentId` INT NOT NULL,
    `Title` VARCHAR(150) NOT NULL,
    `IssuingOrganization` VARCHAR(150) NOT NULL,
    `IssueDate` DATE NOT NULL,
    `FilePath` VARCHAR(255) NOT NULL,
    `LinkedSkillId` INT NOT NULL,
    `VerificationStatus` VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Verified, Rejected
    `VerificationHash` VARCHAR(64) NULL, -- SHA-256 Hash
    `ReviewedByFacultyId` INT NULL,
    `ReviewNotes` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`StudentId`) REFERENCES `StudentProfiles`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`LinkedSkillId`) REFERENCES `Skills`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 12. Internships Table
CREATE TABLE `Internships` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `RecruiterId` INT NOT NULL,
    `Title` VARCHAR(150) NOT NULL,
    `CompanyName` VARCHAR(150) NOT NULL,
    `Location` VARCHAR(150) NOT NULL,
    `Stipend` VARCHAR(100) NULL,
    `Description` TEXT NULL,
    `RequiredSkillIds` VARCHAR(255) NOT NULL, -- Comma-separated Skill IDs
    `MinimumMatchPercentage` INT NOT NULL DEFAULT 60,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`RecruiterId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 13. Applications Table (with Auto Selection rule + Recruiter Override)
CREATE TABLE `Applications` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `InternshipId` INT NOT NULL,
    `StudentId` INT NOT NULL,
    `MatchPercentage` INT NOT NULL,
    `AutoDecision` VARCHAR(50) NOT NULL, -- Selected, Rejected
    `FinalDecision` VARCHAR(50) NOT NULL, -- Selected, Rejected, Pending
    `DecisionDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Notes` TEXT NULL,
    FOREIGN KEY (`InternshipId`) REFERENCES `Internships`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`StudentId`) REFERENCES `StudentProfiles`(`Id`) ON DELETE CASCADE,
    UNIQUE KEY `uk_internship_student` (`InternshipId`, `StudentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 14. Mentorship Table
CREATE TABLE `Mentorships` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `StudentId` INT NOT NULL,
    `FacultyId` INT NULL,
    `Topic` VARCHAR(150) NOT NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Requested', -- Requested, Active, Completed
    `Notes` TEXT NULL,
    `RequestedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`StudentId`) REFERENCES `StudentProfiles`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`FacultyId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 15. Verification Badges (Email, Phone, LinkedIn, GitHub per User)
CREATE TABLE `VerificationBadges` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL UNIQUE,
    `IsEmailVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `IsPhoneVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `IsLinkedInVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `IsGitHubVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
