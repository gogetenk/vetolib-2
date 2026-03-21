import type { Metadata } from "next";
import StockPageClient from "./StockPageClient";

export const metadata: Metadata = {
  title: "Stock",
};

export default function StockPage() {
  return <StockPageClient />;
}
