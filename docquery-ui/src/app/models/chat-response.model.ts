export interface ChatSource {
    document: string;
    pageNumber: number;
}

export interface ChatResponse {
    question: string;
    answer: string;
    sources: ChatSource[];
}