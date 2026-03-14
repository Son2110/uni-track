// REST API client for backend
const API_BASE_URL = import.meta.env.VITE_REST_API_URL;

// Import auth token getter
import { getAuthToken } from "@/features/auth/api/authApi";

// Frontend uses camelCase
interface FrontendApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
  pagination?: {
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
  };
}

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  private async request<T>(
    endpoint: string,
    options?: RequestInit,
  ): Promise<FrontendApiResponse<T>> {
    const url = `${this.baseUrl}${endpoint}`;

    // Build headers
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      ...(options?.headers as Record<string, string>),
    };

    // Add auth token if available
    const authToken = getAuthToken();
    if (authToken) {
      headers["Authorization"] = `Bearer ${authToken}`;
    }

    const response = await fetch(url, {
      ...options,
      headers,
    });

    // Handle 401 Unauthorized - token expired or invalid
    if (response.status === 401) {
      // Clear auth data
      localStorage.removeItem("authToken");
      localStorage.removeItem("authUser");

      // Redirect to login
      window.location.href = "/login";

      throw new Error("Session expired. Please login again.");
    }

    if (!response.ok) {
      const error = await response
        .json()
        .catch(() => ({ message: "An error occurred" }));
      throw new Error(
        error.message || `HTTP error! status: ${response.status}`,
      );
    }

    // Handle 204 No Content responses
    if (response.status === 204) {
      return {
        success: true,
        message: "Success",
        data: null as T,
      };
    }

    const json = await response.json();

    // Backend can return either PascalCase or camelCase wrapped responses
    if (json && typeof json === "object") {
      // Check for wrapped ApiResponse format (both PascalCase and camelCase)
      if (
        "Data" in json ||
        "Success" in json ||
        "data" in json ||
        "success" in json
      ) {
        // Get the data field (try both PascalCase and camelCase)
        let convertedData = (json.Data || json.data) as T;

        // If data is a paginated response object with items array, extract the items
        if (
          convertedData &&
          typeof convertedData === "object" &&
          !Array.isArray(convertedData) &&
          "items" in convertedData &&
          Array.isArray((convertedData as any).items)
        ) {
          // Extract the items array from pagination object
          convertedData = (convertedData as any).items as T;
        }

        // Return the data directly
        return {
          success: json.Success ?? json.success ?? true,
          message: json.Message ?? json.message ?? "Success",
          data: convertedData,
          errors: json.Errors ?? json.errors,
          pagination:
            json.Pagination || json.pagination
              ? {
                  pageNumber:
                    (json.Pagination || json.pagination).PageNumber ||
                    (json.Pagination || json.pagination).pageNumber,
                  pageSize:
                    (json.Pagination || json.pagination).PageSize ||
                    (json.Pagination || json.pagination).pageSize,
                  totalPages:
                    (json.Pagination || json.pagination).TotalPages ||
                    (json.Pagination || json.pagination).totalPages,
                  totalRecords:
                    (json.Pagination || json.pagination).TotalRecords ||
                    (json.Pagination || json.pagination).totalRecords,
                }
              : undefined,
        };
      }

      // If backend returns array directly (legacy endpoints)
      if (Array.isArray(json)) {
        return {
          success: true,
          message: "Success",
          data: json as T,
        };
      }

      // If backend returns object directly without wrapper
      return {
        success: true,
        message: "Success",
        data: json as T,
      };
    }

    return json;
  }

  async get<T>(
    endpoint: string,
    params?: Record<string, string | number | undefined>,
  ): Promise<FrontendApiResponse<T>> {
    const searchParams = new URLSearchParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined) {
          searchParams.append(key, String(value));
        }
      });
    }
    const queryString = searchParams.toString();
    const url = queryString ? `${endpoint}?${queryString}` : endpoint;
    return this.request<T>(url);
  }

  async post<T>(
    endpoint: string,
    data: unknown,
    options?: RequestInit,
  ): Promise<FrontendApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: "POST",
      body: JSON.stringify(data),
      ...options,
    });
  }

  async put<T>(
    endpoint: string,
    data: unknown,
    options?: RequestInit,
  ): Promise<FrontendApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: "PUT",
      body: JSON.stringify(data),
      ...options,
    });
  }

  async patch<T>(
    endpoint: string,
    data: unknown,
    options?: RequestInit,
  ): Promise<FrontendApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: "PATCH",
      body: JSON.stringify(data),
      ...options,
    });
  }

  async delete<T>(
    endpoint: string,
    options?: RequestInit,
  ): Promise<FrontendApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: "DELETE",
      ...options,
    });
  }
}

export const apiClient = new ApiClient(API_BASE_URL);
