# Topic 06 - Software Requirement Specification v.2.3

---

# [Project Name]

# Software Requirement Specification

---

**Class Code:** [ClassCode]  
**Group Code:** [GroupCode]

**[Location], [Date]**

---

# Record of Change

*A - Added | M - Modified | D - Deleted*

| Effective Date | Changed Items | A / M / D | Change Description | New Version |
|----------------|----------------|-----------|--------------------|-------------|
| [Date] | Initial | A | Add project overview | 1.0 |
| [Date] | [Section Name] | M | [Description of change] | 1.1 |

---

# SIGNATURE PAGE

## ORIGINATOR

| Name | Date | Role/Title |
|--------|--------|-------------|
| [Name] | [Date] | [Role/Title] |
| [Name] | [Date] | [Role/Title] |
| [Name] | [Date] | [Role/Title] |
| [Name] | [Date] | [Role/Title] |
| [Name] | [Date] | [Role/Title] |

---

## REVIEWERS

| Name | Date | Role |
|--------|--------|--------|
| [Name] | [Date] | [Mentor/Stakeholder] |

---

# TABLE OF CONTENTS

> Auto-generate this section in your Markdown editor if supported.

---

# 1. Introduction

## 1.1 Purpose

Describe the purpose of this SRS document.  
Example:  
> "This SRS document fully describes the external behavior of the [System Name]."

---

## 1.2 Definitions, Acronyms

List all abbreviations and technical terms used in the document.

- **[Acronym]:** [Definition]  
- **[Term]:** [Definition]

---

## 1.3 References

List external standards, guidelines, or documents referenced.

- IEEE Std 830-1998
- [Domain Specific Regulation]
- [System Architecture Documentation]

---

# 2. Overall Description

## 2.1 Product Perspective

Describe the system context.

- Standalone or part of larger system
- External connections
- Context diagrams

---

## 2.2 Business Process

Provide high-level workflows supported by the system.

- **[Process Name 1]:** [Brief workflow description]
- **[Process Name 2]:** [Brief workflow description]

---

## 2.3 User Classes

### [User Class A] (e.g., Administrator)

- Goals: [What they want to achieve]
- Tasks: [Actions they perform]
- Technical Expertise: Beginner / Intermediate / Advanced

### [User Class B] (e.g., End User)

- Goals: [...]
- Tasks: [...]
- Technical Expertise: [...]

---

# 3. FUNCTIONAL REQUIREMENTS

## 3.1 Use Case Diagram

Insert Overall Use Case Diagram here.

---

## 3.2 Use Case Specifications

> Use the exact table structure below for each Use Case.

---

### USE CASE SPECIFICATION

| Field | Value |
|--------|---------|
| Use-case No. | UC-1 |
| Use-case Version | 1.0 |
| Use-case Name | [Name] |
| Author | [Name(s)] |
| Date | [Date] |
| Priority | [Priority] |
| Primary Actor | [User/Admin] |
| Secondary Actor | [System/Database] |

**Description:**  
[Brief summary]

**Triggers:**  
[Event that starts use case]

**Preconditions:**
- PRE-1. [Condition]
- PRE-2. [Condition]

**Post Conditions:**
- POST-1. [Result]
- POST-2. [Result]

---

### Main Success Scenario

1. Actor action
2. User does X
3. System validates X
4. User enters data
5. System saves data

---

### Alternative Scenario

**1.1 [Scenario Name]**

- User does variation Y
- System responds Z

---

### Exceptions

**E1 — [Error Name]**

- System displays error
- User retries

---

### Relationships

Dependencies on other use cases  
Example: *Must complete UC-1 first*

---

### Business Rules

Reference Business Rules Appendix  
Example: BR-01, BR-05

---

## 3.3 State Diagrams

Insert state diagrams for entities with lifecycle complexity.

Examples:
- Order Status
- User Account Status

---

## 3.4 Data Flow Diagrams

Insert DFDs for critical processes.

---

## 3.5 Logical Data Model

Insert ERD or schema description.

---

# 4. NON-FUNCTIONAL REQUIREMENTS

## 4.1 Usability

- Usability Requirement 1: [e.g., Training time requirements]
- Usability Requirement 2: [e.g., Mobile responsiveness]

---

## 4.2 Reliability

- Reliability Requirement 1: [e.g., Uptime percentage (99.5%)]
- Reliability Requirement 2: [e.g., Data backup frequency]

---

## 4.3 Performance

- Performance Requirement 1: [e.g., Response time < 3 seconds]
- Performance Requirement 2: [e.g., Concurrent user capacity]

---

## 4.4 Reusability

- Reusability Requirement 1: [e.g., Modular architecture]

---

## 4.5 Scalability

- Scalability Requirement 1: [e.g., Ability to handle user growth]

---

# 5. Supporting Information

## 5.1 Appendices

---

## Appendix A — Business Rules Reference

Use format **BR-XX**

### User Authentication & Authorization

- BR-01: [Rule Description]
- BR-02: [Rule Description]

### Data Privacy & Security

- BR-XX: [Rule Description]

### [Domain Category]

- BR-XX: [Rule Description]

---

## Appendix B — Integration Requirements

List external systems or APIs.

---

## Appendix C — Security Requirements

List required protocols and standards.

Examples:

- Encryption standards
- Compliance (GDPR, HIPAA, etc.)

---

**[Project Name]**
