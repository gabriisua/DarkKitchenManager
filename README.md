# DarkKitchenManager

Full-stack B2B management platform for dark kitchens and food production companies.

## Overview

DarkKitchenManager is a modern platform designed to manage food production workflows, orders, logistics, inventory and invoicing processes.

The project replaces a legacy system with a scalable architecture based on:

- Angular frontend
- .NET 10 REST API backend
- Docker Compose orchestration

## Features

- Order management
- Food production workflow management
- Inventory and stock management
- Multitenant authentication
- Invoicing integration
- PDF document generation
- Dynamic label generation
- Direct ZPL label printing through network printers

## Architecture


darkkitchenmanager
│
├── frontend
│ └── Angular application
│
├── backend
│ └── .NET 10 REST API
│
└── docker-compose.yml


## Technologies

### Frontend
- Angular
- TypeScript
- REST API integration

### Backend
- .NET 10
- C#
- Entity Framework Core
- REST API
- N-Tier Architecture

### Infrastructure
- Docker
- Docker Compose

## Configuration

Before running the project, configure the required environment variables and application settings.

Sensitive information must not be committed to the repository, including:

- Database credentials
- API keys
- Authentication secrets
- Printer IP addresses
- Internal network configuration

## Local Development

Run the entire stack using:

```bash
docker compose up