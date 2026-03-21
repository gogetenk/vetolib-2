import type { Metadata } from "next";
import PatientsPageClient from "./PatientsPageClient";

export const metadata: Metadata = {
  title: "Patients",
};

export default function PatientsPage() {
  return <PatientsPageClient />;
}
