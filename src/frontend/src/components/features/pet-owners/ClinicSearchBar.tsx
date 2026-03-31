"use client";

import { useState, useCallback } from "react";
import { Search, MapPin, Star, Loader2 } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { searchClinics, type ClinicSearchResult } from "@/lib/api/pet-owners";

interface Props {
  messages: {
    placeholder: string;
    button: string;
    no_results: string;
    loading: string;
  };
}

export function ClinicSearchBar({ messages }: Props) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<ClinicSearchResult[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  const handleSearch = useCallback(async () => {
    setLoading(true);
    setSearched(true);
    try {
      const data = await searchClinics(query);
      setResults(data);
    } catch {
      setResults([]);
    } finally {
      setLoading(false);
    }
  }, [query]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "Enter") {
        handleSearch();
      }
    },
    [handleSearch]
  );

  return (
    <div data-testid="clinic-search-section" className="w-full">
      <div className="flex gap-2">
        <div className="relative flex-1">
          <Search
            className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400"
            aria-hidden="true"
          />
          <Input
            data-testid="clinic-search-input"
            type="text"
            placeholder={messages.placeholder}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            className="pl-10"
          />
        </div>
        <Button
          data-testid="clinic-search-button"
          onClick={handleSearch}
          disabled={loading}
          className="bg-primary text-white hover:bg-primary/90"
        >
          {loading ? (
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
          ) : (
            messages.button
          )}
        </Button>
      </div>

      {loading && (
        <p
          data-testid="clinic-search-loading"
          className="mt-4 text-sm text-stone-500"
        >
          {messages.loading}
        </p>
      )}

      {!loading && searched && results && results.length === 0 && (
        <p
          data-testid="clinic-search-no-results"
          className="mt-4 text-sm text-stone-500"
        >
          {messages.no_results}
        </p>
      )}

      {!loading && results && results.length > 0 && (
        <ul
          data-testid="clinic-search-results"
          className="mt-4 space-y-3"
        >
          {results.map((clinic) => (
            <li
              key={clinic.id}
              data-testid={`clinic-result-${clinic.id}`}
              className="flex items-start gap-3 rounded-xl border border-stone-100 bg-white p-4 shadow-sm transition-all duration-200 hover:shadow-md hover:-translate-y-0.5"
            >
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
                <MapPin className="h-5 w-5 text-primary" aria-hidden="true" />
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-stone-900">{clinic.name}</p>
                <p className="text-sm text-stone-500">{clinic.address}</p>
                <div className="mt-1 flex items-center gap-2 text-xs text-stone-400">
                  <span className="flex items-center gap-0.5">
                    <Star
                      className="h-3.5 w-3.5 fill-amber-400 text-amber-400"
                      aria-hidden="true"
                    />
                    {clinic.rating}
                  </span>
                  <span>({clinic.reviewCount})</span>
                  <span className="rounded-full bg-stone-100 px-2 py-0.5">
                    {clinic.emirate}
                  </span>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
